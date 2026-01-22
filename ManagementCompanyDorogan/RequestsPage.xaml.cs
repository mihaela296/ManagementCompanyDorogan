using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class RequestsPage : Page
    {
        private string connectionString;

        public RequestsPage()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Не настроено подключение к БД", "Ошибка");
                return;
            }
            LoadRequests();
        }

        private void LoadRequests()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Используем OtchetPoOplate как таблицу заявок
                    string query = @"
                        SELECT 
                            PaymentId as 'ID',
                            Adress as 'Адрес',
                            Period as 'Описание проблемы',
                            Accrued as 'Дата создания',
                            Paid as 'Статус'
                        FROM OtchetPoOplate 
                        ORDER BY Accrued DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgRequests.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка");
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditRequestPage(null));
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }

        private void dgRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRequests.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgRequests.SelectedItem;
                int requestId = Convert.ToInt32(row["ID"]);
                NavigationService.Navigate(new EditRequestPage(requestId));
            }
        }
    }
}