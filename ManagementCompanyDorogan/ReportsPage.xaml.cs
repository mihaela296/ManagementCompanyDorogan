using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class ReportsPage : Page
    {
        private string connectionString;

        public ReportsPage()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadReports();
        }

        private void LoadReports()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            PaymentId as 'ID',
                            Adress as 'Адрес',
                            Period as 'Период',
                            Accrued as 'Начислено',
                            ISNULL(Paid, 0) as 'Оплачено'
                        FROM OtchetPoOplate 
                        ORDER BY Period DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgReports.ItemsSource = dt.DefaultView;

                    // Обновление статистики
                    UpdateStatistics(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateStatistics(DataTable dt)
        {
            int total = dt.Rows.Count;
            int paid = 0;
            int debts = 0;
            decimal totalSum = 0;

            foreach (DataRow row in dt.Rows)
            {
                decimal accrued = row["Начислено"] != DBNull.Value ? Convert.ToDecimal(row["Начислено"]) : 0;
                decimal paidAmount = row["Оплачено"] != DBNull.Value ? Convert.ToDecimal(row["Оплачено"]) : 0;

                totalSum += accrued;

                if (paidAmount >= accrued) paid++;
                if (paidAmount < accrued) debts++;
            }

            txtTotal.Text = $"Всего: {total} записей";
            txtSum.Text = $"Сумма: {totalSum:N2} ₽";
            txtPaid.Text = $"Оплачено: {paid}";
            txtDebts.Text = $"Долги: {debts}";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadReports();
        }
    }
}