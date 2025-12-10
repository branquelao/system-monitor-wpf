using System.Text;
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
using SystemMonitorWpf.Services;

namespace SystemMonitorWPF
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCpu();
            UpdateRam();
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
            double percent = ram.Percent;

            TxtRam.Text = $"RAM: {percent:F2}% ({usedMb:F2} / ({totalMb:F2}%)";
            BarRam.Value = percent;
        }
    }
}