using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

using SystemMonitorWpf.Services;
using SystemMonitorWPF.Services;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;

namespace SystemMonitorWPF
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<ProcessInfo> Processes { get; set; }
            = new ObservableCollection<ProcessInfo>();

        private readonly DispatcherTimer _timer;
        private readonly ProcessCpuTracker _cpuTracker = new ProcessCpuTracker();
        private readonly Dictionary<string, int> _sortStates = new();
        public ICollectionView ProcessesView { get; set; }
        public ProcessInfo SelectedProcess { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            ProcessesView = CollectionViewSource.GetDefaultView(Processes);
            ProcessesView.SortDescriptions.Add(
                new SortDescription(nameof(ProcessInfo.CpuUsage), ListSortDirection.Descending));
            ProcessesView.SortDescriptions.Add(
                new SortDescription(nameof(ProcessInfo.MemoryUsage), ListSortDirection.Descending));
            ProcessesView.SortDescriptions.Add(
                new SortDescription(nameof(ProcessInfo.Name), ListSortDirection.Ascending));

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

        private void KillProcess(object sender, RoutedEventArgs e)
        {
            if (SelectedProcess == null)
            {
                MessageBox.Show("No process selected.",
                                "Notice",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            try
            {
                var p = Process.GetProcessById(SelectedProcess.PID);
                p.Kill();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to kill process: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void GridProcessesSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            var view = ProcessesView;
            string property = e.Column.SortMemberPath;

            if (string.IsNullOrEmpty(property))
                return;

            if (!_sortStates.ContainsKey(property))
                _sortStates[property] = 0;

            _sortStates[property] = (_sortStates[property] + 1) % 3;

            view.SortDescriptions.Clear();

            switch (_sortStates[property])
            {
                case 1: // Descending
                    view.SortDescriptions.Add(
                        new SortDescription(property, ListSortDirection.Descending));
                    e.Column.SortDirection = ListSortDirection.Descending;
                    break;

                case 2: // Ascending
                    view.SortDescriptions.Add(
                        new SortDescription(property, ListSortDirection.Ascending));
                    e.Column.SortDirection = ListSortDirection.Ascending;
                    break;

                default: // Reset
                    ResetDefaultSort();
                    e.Column.SortDirection = null;
                    break;
            }

            foreach (var col in GridProcesses.Columns)
            {
                if (col != e.Column)
                    col.SortDirection = null;
            }

            view.Refresh();
        }

        private void ResetDefaultSort()
        {
            ProcessesView.SortDescriptions.Clear();

            ProcessesView.SortDescriptions.Add(
                new SortDescription(nameof(ProcessInfo.CpuUsage), ListSortDirection.Descending));
            ProcessesView.SortDescriptions.Add(
                new SortDescription(nameof(ProcessInfo.MemoryUsage), ListSortDirection.Descending));
            ProcessesView.SortDescriptions.Add(
                new SortDescription(nameof(ProcessInfo.Name), ListSortDirection.Ascending));
        }
    }
}
