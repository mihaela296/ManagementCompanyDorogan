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
            // Открываем страницу отчета по задолженностям
            NavigationService.Navigate(new OtchetDebtPage());
        }

        private void btnReturnToStart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }
}