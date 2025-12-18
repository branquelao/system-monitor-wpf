using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Threading;
using SystemMonitorWpf.Services;

namespace SystemMonitorWPF.ViewModels
{
    public class MemoryViewModel : INotifyPropertyChanged
    {
        private double _ramUsage;

        public double RamUsage
        {
            get => _ramUsage;
            set
            {
                _ramUsage = value;
                OnPropertyChanged(nameof(RamUsage));
                OnPropertyChanged(nameof(RamText));
            }
        }

        public string RamText { get; private set; } = "";

        private readonly DispatcherTimer _timer;

        public MemoryViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (_, _) =>
            {
                var ram = MemoryInfo.GetRam();
                RamUsage = ram.Percent;

                double totalMb = ram.Total / 1024.0 / 1024.0;
                double usedMb = ram.Used / 1024.0 / 1024.0;

                RamText = $"RAM: {ram.Percent:F2}% ({usedMb:F2} / {totalMb:F2} MB)";
                OnPropertyChanged(nameof(RamText));
            };

            _timer.Start();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
