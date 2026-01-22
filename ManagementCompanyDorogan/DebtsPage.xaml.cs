using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using Microsoft.Win32;
using System.IO;

namespace ManagementCompanyDorogan
{
    public partial class DebtsPage : Page
    {
        private string connectionString;
        private DataTable debtsTable;

        public DebtsPage()
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
            LoadDebts();
        }

        private void LoadDebts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            Debt_id as 'ID',
                            Adress as 'Адрес',
                            Apartment as 'Квартира',
                            Water as 'Вода',
                            Electricity as 'Электричество',
                            Phone as 'Телефон'
                        FROM Debt
                        ORDER BY Adress";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    debtsTable = new DataTable();
                    adapter.Fill(debtsTable);

                    dgDebts.ItemsSource = debtsTable.DefaultView;

                    CalculateTotals();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки долгов:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotals()
        {
            if (debtsTable == null) return;

            decimal totalWater = 0;
            decimal totalElectricity = 0;
            decimal totalDebt = 0;
            int count = debtsTable.Rows.Count;

            foreach (DataRow row in debtsTable.Rows)
            {
                decimal water = row["Вода"] != DBNull.Value ? Convert.ToDecimal(row["Вода"]) : 0;
                decimal electricity = row["Электричество"] != DBNull.Value ? Convert.ToDecimal(row["Электричество"]) : 0;

                totalWater += water;
                totalElectricity += electricity;
                totalDebt += water + electricity;
            }

            txtTotalWater.Text = $"Вода: {totalWater:N2} ₽";
            txtTotalElectricity.Text = $"Электричество: {totalElectricity:N2} ₽";
            txtTotalDebt.Text = $"Всего: {totalDebt:N2} ₽";
            txtDebtCount.Text = $" ({count} записей)";

            if (totalDebt > 0)
                txtTotalDebt.Foreground = System.Windows.Media.Brushes.Red;
            else
                txtTotalDebt.Foreground = System.Windows.Media.Brushes.Green;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDebts();
        }

        private void btnLargeDebts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            d.Adress,
                            ISNULL(d.Water, 0) + ISNULL(d.Electricity, 0) as TotalDebt
                        FROM Debt d
                        WHERE ISNULL(d.Water, 0) + ISNULL(d.Electricity, 0) > 10000
                        ORDER BY TotalDebt DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    string largeDebts = "Крупные задолженности (>10,000 ₽):\n\n";
                    bool hasDebts = false;

                    while (reader.Read())
                    {
                        hasDebts = true;
                        largeDebts += $"{reader["Adress"]}\n";
                        largeDebts += $"  Долг: {Convert.ToDecimal(reader["TotalDebt"]):N2} ₽\n\n";
                    }
                    reader.Close();

                    if (!hasDebts)
                        largeDebts = "Крупных задолженностей не найдено.";

                    MessageBox.Show(largeDebts, "Крупные долги",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt";
                saveDialog.FileName = $"Долги {DateTime.Now:ddMMyyyy}.txt";

                if (saveDialog.ShowDialog() == true)
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("СПИСОК ЗАДОЛЖЕННОСТЕЙ");
                        writer.WriteLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                        writer.WriteLine("=".PadRight(80, '='));
                        writer.WriteLine();

                        foreach (DataRow row in debtsTable.Rows)
                        {
                            writer.WriteLine($"Адрес: {row["Адрес"]}");
                            writer.WriteLine($"Квартира: {row["Квартира"]}");
                            writer.WriteLine($"Вода: {row["Вода"]} ₽");
                            writer.WriteLine($"Электричество: {row["Электричество"]} ₽");

                            decimal water = row["Вода"] != DBNull.Value ? Convert.ToDecimal(row["Вода"]) : 0;
                            decimal electricity = row["Электричество"] != DBNull.Value ? Convert.ToDecimal(row["Электричество"]) : 0;
                            writer.WriteLine($"ИТОГО: {(water + electricity):N2} ₽");
                            writer.WriteLine("-".PadRight(40, '-'));
                        }
                    }

                    MessageBox.Show($"Данные экспортированы в файл:\n{saveDialog.FileName}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}