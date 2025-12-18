using System.Windows;
using SystemMonitorWPF.ViewModels;

namespace SystemMonitorWPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
