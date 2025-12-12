using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

using SystemMonitorWpf.Services;
using SystemMonitorWPF.Services;

namespace SystemMonitorWPF
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<ProcessInfo> Processes { get; set; }
            = new ObservableCollection<ProcessInfo>();

        private readonly DispatcherTimer _timer;
        private readonly ProcessCpuTracker _cpuTracker = new ProcessCpuTracker();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCpu();
            UpdateRam();
            UpdateProcesses();
        }

        private void UpdateCpu()
        {
            double cpu = CpuInfo.GetCpuUsage();
            TxtCpu.Text = $"CPU: {cpu:F2}%";
            BarCpu.Value = cpu;
        }

        private void UpdateRam()
        {
            var ram = MemoryInfo.GetRam();

            double totalMb = ram.Total / 1024.0 / 1024.0;
            double usedMb = ram.Used / 1024.0 / 1024.0;

            TxtRam.Text = $"RAM: {ram.Percent:F2}% ({usedMb:F2} / {totalMb:F2} MB)";
            BarRam.Value = ram.Percent;
        }

        private void UpdateProcesses()
        {
            var running = Process.GetProcesses();
            var toRemove = Processes
                .Where(pr => !running.Any(p => p.Id == pr.PID))
                .ToList();

            foreach (var r in toRemove)
                Processes.Remove(r);

            foreach (var p in running)
            {
                try
                {
                    if (!ShouldShowProcess(p))
                        continue;

                    double cpu = _cpuTracker.GetCpu(p);
                    double memory = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 2);

                    var existing = Processes.FirstOrDefault(x => x.PID == p.Id);

                    if (existing == null)
                    {
                        Processes.Add(new ProcessInfo
                        {
                            PID = p.Id,
                            Name = p.ProcessName,
                            CpuUsage = cpu,
                            MemoryUsage = memory
                        });
                    }
                    else
                    {
                        existing.CpuUsage = cpu;
                        existing.MemoryUsage = memory;
                    }
                }
                catch { }
            }
        }

        private bool ShouldShowProcess(Process p)
        {
            try
            {
                string name = p.ProcessName.ToLower();

                if(string.IsNullOrWhiteSpace(name))
                    return false;

                string[] systemNames = new[]
                {
                    "system",
                    "idle",
                    "registry",
                    "smss",
                    "csrss",
                    "wininit",
                    "winlogon",
                    "services",
                    "lsass",
                    "svchost",
                    "fontdrvhost",
                    "memory compression"
                };

                if (systemNames.Contains(name))
                    return false;
                
                if(p.WorkingSet64 < 5 * 1024 * 1024 && _cpuTracker.GetCpu(p) < 0.1)
                    return false;

                if(p.StartTime.AddSeconds(2) > DateTime.Now)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
