using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class OwnersPage : Page
    {
        private string connectionString;

        public OwnersPage()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Не настроено подключение к БД. Проверьте App.config",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LoadOwnersData();
        }

        private void LoadOwnersData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Owners ORDER BY Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgOwners.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки владельцев: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}