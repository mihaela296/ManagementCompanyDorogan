using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class RequestHistoryPage : Page
    {
        private string connectionString;
        private DataTable historyTable;

        public RequestHistoryPage()
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
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            PaymentId as 'ID',
                            Adress as 'Адрес',
                            Period as 'Описание',
                            Accrued as 'Дата_создания',
                            Paid as 'Статус_код'
                        FROM OtchetPoOplate
                        WHERE Paid IN (0, 1, 2)
                        ORDER BY Accrued DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    historyTable = new DataTable();
                    adapter.Fill(historyTable);

                    dgHistory.ItemsSource = historyTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            // Простой диалог фильтрации
            Window dialog = new Window
            {
                Title = "Фильтр по статусу",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };

            panel.Children.Add(new TextBlock
            {
                Text = "Введите статус (0-Открыта, 1-В работе, 2-Закрыта):",
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
                Content = "Применить",
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
                if (int.TryParse(textBox.Text, out int statusCode) && statusCode >= 0 && statusCode <= 2)
                {
                    DataView view = historyTable.DefaultView;
                    view.RowFilter = $"[Статус_код] = {statusCode}";
                    dgHistory.ItemsSource = view;
                }
                else
                {
                    MessageBox.Show("Введите число от 0 до 2", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
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

            dialog.ShowDialog();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (historyTable != null)
            {
                historyTable.DefaultView.RowFilter = "";
                dgHistory.ItemsSource = historyTable.DefaultView;
            }
        }

        private void btnStatistics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (historyTable == null || historyTable.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для статистики",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                int total = historyTable.Rows.Count;
                int open = 0, inProgress = 0, closed = 0;

                foreach (DataRow row in historyTable.Rows)
                {
                    if (row["Статус_код"] != DBNull.Value)
                    {
                        decimal status = Convert.ToDecimal(row["Статус_код"]);
                        if (status == 0) open++;
                        else if (status == 1) inProgress++;
                        else if (status == 2) closed++;
                    }
                }

                string stats = $"Статистика заявок:\n\n";
                stats += $"Всего заявок: {total}\n";
                stats += $"Открытых: {open}\n";
                stats += $"В работе: {inProgress}\n";
                stats += $"Закрытых: {closed}";

                MessageBox.Show(stats, "Статистика",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета статистики:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}