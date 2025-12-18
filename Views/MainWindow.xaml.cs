using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SystemMonitorWPF.Services;
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

        private void KillProcess(object sender, RoutedEventArgs e)
        {
            var tabControl = this.FindName("MainTabControl") as TabControl;
            if (tabControl == null)
                return;

            DataGrid? activeGrid = null;

            if (tabControl.SelectedIndex == 0)
                activeGrid = this.FindName("CpuGrid") as DataGrid;
            else if (tabControl.SelectedIndex == 1)
                activeGrid = this.FindName("RamGrid") as DataGrid;

            if (activeGrid?.SelectedItem is not ProcessInfo selected)
            {
                MessageBox.Show("No process selected.",
                                "Notice",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            try
            {
                Process.GetProcessById(selected.PID).Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to kill process:\n{ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

    }
}
