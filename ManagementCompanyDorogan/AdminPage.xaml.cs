using System.Windows;
using System.Windows.Controls;

namespace ManagementCompanyDorogan
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private void SpisokButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SpisokPage());
        }

        private void DebtButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DebtsPage());
        }

        private void OtchetButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ReportsPage());
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            // Используем существующую страницу отчета по оплате
            NavigationService.Navigate(new ReportsPage());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки системы доступны системному администратору",
                          "Управление",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnReturnToStart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }
}