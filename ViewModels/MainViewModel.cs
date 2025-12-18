using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Threading;
using SystemMonitorWpf.Services;
using SystemMonitorWPF.Services;

namespace SystemMonitorWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ProcessInfo> Processes { get; }
            = new ObservableCollection<ProcessInfo>();

        public ICollectionView ProcessesView { get; }

        private readonly DispatcherTimer _timer;
        private readonly ProcessCpuTracker _cpuTracker = new();

        private double _cpuUsage;
        public double CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged(nameof(CpuUsage));
            }
        }

        private double _ramPercent;
        public double RamPercent
        {
            get => _ramPercent;
            set
            {
                _ramPercent = value;
                OnPropertyChanged(nameof(RamPercent));
            }
        }

        private string _ramText;
        public string RamText
        {
            get => _ramText;
            set
            {
                _ramText = value;
                OnPropertyChanged(nameof(RamText));
            }
        }

        public MainViewModel()
        {
            ProcessesView = CollectionViewSource.GetDefaultView(Processes);

            ProcessesView.SortDescriptions.Add(
                new SortDescription(nameof(ProcessInfo.CpuUsage), ListSortDirection.Descending));
            ProcessesView.SortDescriptions.Add(
                new SortDescription(nameof(ProcessInfo.MemoryUsage), ListSortDirection.Descending));
            ProcessesView.SortDescriptions.Add(
                new SortDescription(nameof(ProcessInfo.Name), ListSortDirection.Ascending));

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (_, _) =>
            {
                UpdateCpu();
                UpdateRam();
                UpdateProcesses();
            };

            _timer.Start();
        }

        private void UpdateCpu()
        {
            CpuUsage = CpuInfo.GetCpuUsage();
        }

        private void UpdateRam()
        {
            var ram = MemoryInfo.GetRam();

            double totalMb = ram.Total / 1024.0 / 1024.0;
            double usedMb = ram.Used / 1024.0 / 1024.0;

            RamPercent = ram.Percent;
            RamText = $"{usedMb:F2} / {totalMb:F2} MB";
        }

        private void UpdateProcesses()
        {
            var running = Process.GetProcesses();

            var toRemove = Processes
                .Where(p => !running.Any(r => r.Id == p.PID))
                .ToList();

            foreach (var r in toRemove)
                Processes.Remove(r);

            foreach (var p in running)
            {
                try
                {
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
