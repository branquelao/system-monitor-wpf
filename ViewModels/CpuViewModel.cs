using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Threading;
using SystemMonitorWpf.Services;

namespace SystemMonitorWPF.ViewModels
{
    public class CpuViewModel : INotifyPropertyChanged
    {
        private double _cpuUsage;

        public double CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged(nameof(CpuUsage));
                OnPropertyChanged(nameof(CpuText));
            }
        }

        public string CpuText => $"CPU: {CpuUsage:F2}%";

        private readonly DispatcherTimer _timer;

        public CpuViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (_, _) =>
            {
                CpuUsage = CpuInfo.GetCpuUsage();
            };

            _timer.Start();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

}
