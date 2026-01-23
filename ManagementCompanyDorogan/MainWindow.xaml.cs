using System.Windows;
using System.Windows.Controls;

namespace ManagementCompanyDorogan
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadStartPage();
        }

        private void LoadStartPage()
        {
            MainPage mainPage = new MainPage();
            mainPage.RoleSelected += OnRoleSelected;
            MainFrame.Navigate(mainPage);
            UpdateBackButton();
            UpdateTitle();
        }

        private void OnRoleSelected(object sender, string role)
        {
            if (role.Contains("Администратор"))
            {
                MainFrame.Navigate(new AdminPage());
            }
            else if (role.Contains("Руководитель"))
            {
                MainFrame.Navigate(new DirectorPage());
            }
            UpdateBackButton();
            UpdateTitle();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
                UpdateBackButton();
                UpdateTitle();
            }
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateBackButton();
            UpdateTitle();
        }

        private void UpdateBackButton()
        {
            btnBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateTitle()
        {
            if (MainFrame.Content is Page currentPage)
            {
                txtTitle.Text = currentPage.Title;
            }
        }
    }
}