using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class PaymentsPage : Page
    {
        private string connectionString;
        private DataTable paymentsTable;

        public PaymentsPage()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPayments();
        }

        private void LoadPayments()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM OtchetPoOplate ORDER BY Period DESC";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgPayments.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка");
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPayments();
        }

        private void btnFilterPeriod_Click(object sender, RoutedEventArgs e)
        {
            // Простой диалог с TextBox
            Window dialog = new Window
            {
                Title = "Фильтр по периоду",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };

            panel.Children.Add(new TextBlock
            {
                Text = "Введите период (например, Март 2025):",
                Margin = new Thickness(0, 0, 0, 10)
            });

            TextBox textBox = new TextBox { Height = 25 };
            panel.Children.Add(textBox);

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };

            Button okButton = new Button
            {
                Content = "ОК",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };

            Button cancelButton = new Button
            {
                Content = "Отмена",
                Width = 80,
                IsCancel = true
            };

            okButton.Click += (s, args) =>
            {
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    ApplyFilter(textBox.Text);
                }
                dialog.DialogResult = true;
            };

            cancelButton.Click += (s, args) =>
            {
                dialog.DialogResult = false;
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;

            if (dialog.ShowDialog() == true)
            {
                // Фильтр применен
            }
        }

        private void ApplyFilter(string period)
        {
            if (paymentsTable != null)
            {
                DataView view = paymentsTable.DefaultView;
                view.RowFilter = $"Period = '{period.Replace("'", "''")}'";
            }
        }

        private void btnSummary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            Period,
                            COUNT(*) as Количество,
                            SUM(Accrued) as Начислено,
                            SUM(ISNULL(Paid, 0)) as Оплачено
                        FROM OtchetPoOplate
                        GROUP BY Period
                        ORDER BY Period DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    string summary = "Сводка по периодам:\n\n";
                    while (reader.Read())
                    {
                        summary += $"{reader["Period"]}:\n";
                        summary += $"  Количество: {reader["Количество"]}\n";
                        summary += $"  Начислено: {Convert.ToDecimal(reader["Начислено"]):N2} ₽\n";
                        summary += $"  Оплачено: {Convert.ToDecimal(reader["Оплачено"]):N2} ₽\n\n";
                    }
                    reader.Close();

                    if (summary.Length <= 25)
                        summary = "Нет данных для сводки";

                    MessageBox.Show(summary, "Итоговая сводка",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета сводки:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}