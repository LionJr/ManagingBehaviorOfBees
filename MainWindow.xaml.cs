using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ManagingBehaviorOfBees
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private DispatcherTimer timer = new DispatcherTimer();
    private Queen queen = new Queen();
    public MainWindow()
    {
      InitializeComponent();
      statusReport.Text = queen.StatusReport;

      timer.Tick += Timer_Tick;
      timer.Interval = TimeSpan.FromSeconds(1.5);
      timer.Start();
    }
    private void Timer_Tick(object? sender, EventArgs e)
    {
      WorkShift_Click(this,new RoutedEventArgs());
    }
    private void WorkShift_Click(object sender, RoutedEventArgs e)
    {
      queen.WorkTheNextShift();
      statusReport.Text = queen.StatusReport;
    }
    private void AssingJob_Click(object sender, RoutedEventArgs e)
    {
      queen.AssingBee(jobSelector.Text);
      statusReport.Text = queen.StatusReport;
    }
  }

  public static class HoneyVault
  {
    public const float NECTAR_CONVERTION_RATION = 0.19F;
    public const float LOW_LEVEL_WARNING = 10F;
    private static float honey = 25F;
    private static float nectar = 100F;
    public static void CollectNectar(float amount)
    {
      if(amount > 0F)
        nectar+=amount;
    }
    public static void ConvertNectarToHoney(float amount)
    {
      if(amount > nectar)
      {
        amount=nectar;
        nectar-=nectar;
      }
      else
        nectar-=amount;

      honey+=amount*NECTAR_CONVERTION_RATION;
    }
    public static bool ConsumeHoney(float amount)
    {
      if(amount <= honey)
      {
        honey-=amount;
        return true;
      } 
      else
        return false;
    }
    public static string StatusReport 
    {
      get
      {
        string status = $"{honey} units of honey\n" + $"{nectar} units of nectar\n";
        if(honey > LOW_LEVEL_WARNING && nectar > LOW_LEVEL_WARNING)
          return status;
        else if(honey <= LOW_LEVEL_WARNING && nectar > LOW_LEVEL_WARNING)
          return status + "LOW HONEY — ADD A HONEY MANUFACTURER";
        else if(honey > LOW_LEVEL_WARNING && nectar <= LOW_LEVEL_WARNING)
          return status + "LOW NECTAR — ADD A NECTAR COLLECTOR";
        else
          return status + "LOW HONEY AND NECTAR — ADD A HONEY MANUFACTURER AND A NECTAR COLLECTOR";
      } 
    }
  }
  class Bee
  {
    public virtual float CostPerShift {get;}
    public string Job {get; private set;}
    public Bee(string job)
    {
      Job = job;
    }
    public void WorkTheNextShift()
    {
      if(HoneyVault.ConsumeHoney(CostPerShift))
        DoJob();
    }
    protected virtual void DoJob() { /* This method will be overrided by subclass */ }
  }
  class NectarCollector : Bee
  {
    public const float NECTAR_COLLECTED_PER_SHIFT = 33.25F;
    private float costPerShift = 1.95F;
    public override float CostPerShift
    {
      get {return costPerShift;}
    }
    public NectarCollector() : base("Nectar Collector") { }
    protected override void DoJob()
    {
      HoneyVault.CollectNectar(NECTAR_COLLECTED_PER_SHIFT);
    }
  }
  class HoneyManufacturer : Bee
  {
    public const float NECTAR_PROCESSED_PER_SHIFT = 33.15F;
    private float costPerShift = 1.7F;
    public override float CostPerShift
    {
      get {return costPerShift;}
    }
    public HoneyManufacturer() : base("Honey Manufacturer") { }
    protected override void DoJob()
    {
      HoneyVault.ConvertNectarToHoney(NECTAR_PROCESSED_PER_SHIFT);
    }
  }
  class Queen : Bee
  {
    public const float EGGS_PER_SHIFT = 0.45F;
    public const float HONEY_PER_UNASSIGNED_WORKER = 0.5F;
    private Bee[] workers = new Bee[0];
    private float eggs = 0;
    private float unassignedWorkers = 3;
    public string StatusReport {get; private set;} = "";
    private float costPerShift = 2.15F;
    public override float CostPerShift
    {
      get {return costPerShift;}
    }
    public Queen() : base("Queen")
    {
      AssingBee("Nectar Collector");
      AssingBee("Honey Manufacturer");
      AssingBee("Egg Care");
    }
    private void AddWorker(Bee worker)
    {
      if(unassignedWorkers >= 1)
      {
        unassignedWorkers--;
        Array.Resize(ref workers, workers.Length+1);
        workers[workers.Length-1] = worker;
      }
    }
    private string WorkerStatus(string job)
    {
      int count = 0;
      foreach(Bee worker in workers)
        if(worker.Job==job) count++;
      string s = "s";
      if(count==1) s="";
      return $"{count} {job} bee{s}";
    }
    private void UpdateStatusReport()
    {
      StatusReport = $"Vault report:\n{HoneyVault.StatusReport}\n" 
                     + $"\nEgg count: {eggs:0.0}\nUnassigned workers: {unassignedWorkers:0.0}\n"
                     + $"{WorkerStatus("Nectar Collector")}\n{WorkerStatus("Honey Manufacturer")}"
                     + $"\n{WorkerStatus("Egg Care")}\nTOTAL WORKERS: {workers.Length}";
    }
    public void CareForEggs(float eggsToConvert)
    {
      if(eggs >= eggsToConvert)
      {
        eggs -= eggsToConvert;
        unassignedWorkers += eggsToConvert;
      }
    }
    public void AssingBee(string job)
    {
      switch(job)
      {
        case "Nectar Collector":
          AddWorker(new NectarCollector());
          break;
        case "Honey Manufacturer":
          AddWorker(new HoneyManufacturer());
          break;
        case "Egg Care":
          AddWorker(new EggCare(this));
          break;
      }
      UpdateStatusReport();
    }
    protected override void DoJob()
    {
      eggs += EGGS_PER_SHIFT;
      foreach(Bee worker in workers)
      {
        worker.WorkTheNextShift();
      }
      HoneyVault.ConsumeHoney(unassignedWorkers * HONEY_PER_UNASSIGNED_WORKER);
      UpdateStatusReport();
    }
  }
  class EggCare : Bee
  {
    public const float CARE_PROGRESS_PER_SHIFT = 0.15F;
    private float costPerShift = 1.35F;
    public override float CostPerShift
    {
      get {return costPerShift;}
    }
    private Queen queen;
    public EggCare(Queen queen) : base("Egg Care")
    { 
      this.queen = queen;
    }
    protected override void DoJob()
    {
      queen.CareForEggs(CARE_PROGRESS_PER_SHIFT);
    }
  }
}
