using System;
using System.Windows;
using System.Windows.Controls;

namespace ManagementCompanyDorogan
{
    public partial class MainPage : Page
    {
        // Событие для передачи выбранной роли
        public event EventHandler<string> RoleSelected;

        public MainPage()
        {
            InitializeComponent();
        }

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            RoleSelected?.Invoke(this, "Администратор");
        }

        private void btnDirector_Click(object sender, RoutedEventArgs e)
        {
            RoleSelected?.Invoke(this, "Руководитель");
        }
    }
}