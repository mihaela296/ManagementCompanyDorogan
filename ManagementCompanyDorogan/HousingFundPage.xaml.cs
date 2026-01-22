using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class HousingFundPage : Page
    {
        private string connectionString;
        private DataTable housingTable;

        public HousingFundPage()
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
            LoadHousingFund();
        }

        private void LoadHousingFund()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM SpisokJilogoFonda";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    housingTable = new DataTable();
                    adapter.Fill(housingTable);

                    dgHousing.ItemsSource = housingTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка");
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadHousingFund();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (housingTable == null) return;

            string searchText = txtSearch.Text.Trim();
            DataView view = housingTable.DefaultView;

            if (string.IsNullOrEmpty(searchText))
            {
                view.RowFilter = "";
            }
            else
            {
                view.RowFilter = $"Adress LIKE '%{searchText.Replace("'", "''")}%'";
            }
        }
    }
}