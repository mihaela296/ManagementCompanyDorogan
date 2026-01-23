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
    public partial class OtchetDebtPage : Page
    {
        private string connectionString;

        public OtchetDebtPage()
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
            LoadDebtReport();
        }

        private void LoadDebtReport()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            d.Adress,
                            o.Name as OwnerName,
                            d.Apartment,
                            ISNULL(d.Water, 0) as Water,
                            ISNULL(d.Electricity, 0) as Electricity,
                            (ISNULL(d.Water, 0) + ISNULL(d.Electricity, 0)) as TotalDebt
                        FROM Debt d
                        LEFT JOIN Owners o ON d.Owner = o.Owner_id
                        WHERE ISNULL(d.Water, 0) + ISNULL(d.Electricity, 0) > 0
                        ORDER BY TotalDebt DESC, d.Adress";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgDebtReport.ItemsSource = dt.DefaultView;
                    CalculateStatistics(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateStatistics(DataTable dt)
        {
            decimal totalWater = 0;
            decimal totalElectricity = 0;
            decimal totalAll = 0;
            int debtorsCount = dt.Rows.Count;

            foreach (DataRow row in dt.Rows)
            {
                decimal water = row["Water"] != DBNull.Value ? Convert.ToDecimal(row["Water"]) : 0;
                decimal electricity = row["Electricity"] != DBNull.Value ? Convert.ToDecimal(row["Electricity"]) : 0;
                decimal total = water + electricity;

                totalWater += water;
                totalElectricity += electricity;
                totalAll += total;
            }

            txtTotalDebtors.Text = $"Всего должников: {debtorsCount}";
            txtTotalWater.Text = $"Вода: {totalWater:N2} ₽";
            txtTotalElectricity.Text = $"Электричество: {totalElectricity:N2} ₽";
            txtTotalAll.Text = $"Общая сумма: {totalAll:N2} ₽";
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDebtReport();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv";
                saveDialog.FileName = $"Отчет_по_задолженностям_{DateTime.Now:ddMMyyyy}.txt";

                if (saveDialog.ShowDialog() == true)
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("ОТЧЕТ ПО ЗАДОЛЖЕННОСТЯМ");
                        writer.WriteLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                        writer.WriteLine(new string('=', 80));
                        writer.WriteLine();

                        writer.WriteLine($"Всего должников: {txtTotalDebtors.Text.Replace("Всего должников: ", "")}");
                        writer.WriteLine($"Общая сумма задолженностей: {txtTotalAll.Text.Replace("Общая сумма: ", "")}");
                        writer.WriteLine();

                        writer.WriteLine("СПИСОК ЗАДОЛЖЕННОСТЕЙ:");
                        writer.WriteLine(new string('-', 80));

                        if (dgDebtReport.ItemsSource is DataView dataView)
                        {
                            // Заголовки
                            writer.WriteLine(string.Join("\t",
                                "Адрес",
                                "Владелец",
                                "Квартира",
                                "Вода (₽)",
                                "Электричество (₽)",
                                "Общий долг (₽)"));

                            // Данные
                            foreach (DataRowView row in dataView)
                            {
                                writer.WriteLine(string.Join("\t",
                                    row["Adress"],
                                    row["OwnerName"],
                                    row["Apartment"],
                                    $"{Convert.ToDecimal(row["Water"]):N2}",
                                    $"{Convert.ToDecimal(row["Electricity"]):N2}",
                                    $"{Convert.ToDecimal(row["TotalDebt"]):N2}"));
                            }
                        }

                        writer.WriteLine();
                        writer.WriteLine(new string('=', 80));
                        writer.WriteLine("Конец отчета");
                    }

                    MessageBox.Show($"Отчет успешно экспортирован в файл:\n{saveDialog.FileName}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                            o.Name as OwnerName,
                            ISNULL(d.Water, 0) + ISNULL(d.Electricity, 0) as TotalDebt
                        FROM Debt d
                        LEFT JOIN Owners o ON d.Owner = o.Owner_id
                        WHERE ISNULL(d.Water, 0) + ISNULL(d.Electricity, 0) > 10000
                        ORDER BY TotalDebt DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    string largeDebts = "КРУПНЫЕ ЗАДОЛЖЕННОСТИ (более 10,000 ₽):\n\n";
                    bool hasLargeDebts = false;
                    int count = 0;

                    while (reader.Read())
                    {
                        hasLargeDebts = true;
                        count++;
                        largeDebts += $"{count}. {reader["Adress"]}\n";
                        largeDebts += $"   Владелец: {reader["OwnerName"]}\n";
                        largeDebts += $"   Долг: {Convert.ToDecimal(reader["TotalDebt"]):N2} ₽\n\n";
                    }
                    reader.Close();

                    if (!hasLargeDebts)
                    {
                        largeDebts = "Крупных задолженностей (более 10,000 ₽) не найдено.";
                    }

                    MessageBox.Show(largeDebts, "Крупные задолженности",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки крупных задолженностей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}