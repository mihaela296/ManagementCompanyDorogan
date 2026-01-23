using System;
using System.Windows;
using System.Windows.Controls;



namespace ManagementCompanyDorogan
{
    public partial class DirectorPage : Page
    {
        public DirectorPage()
        {
            InitializeComponent();
        }

        private void SpisokButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new SpisokPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перехода на страницу Жилого фонда: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DebtButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new DebtsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перехода на страницу Задолженностей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OtchetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new ReportsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перехода на страницу Отчётов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, существует ли страница ApartPage
                var apartmentsPage = new ApartPage();
                NavigationService.Navigate(apartmentsPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перехода на страницу Квартир: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OwnerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, существует ли страница OwnersPage
                var ownersPage = new OwnersPage();
                NavigationService.Navigate(ownersPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перехода на страницу Владельцев: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // УДАЛЕНО: Метод ReportButton_Click (аналитика)

        private void DebtReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Используем страницу отчетов вместо OtchetDebtPage
                NavigationService.Navigate(new ReportsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перехода на страницу Отчётов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReturnToStart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }
}
