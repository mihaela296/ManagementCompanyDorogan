using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace ManagementCompanyDorogan
{
    public partial class ReportsPage : Page
    {
        private string connectionString;
        private int selectedPaymentId = 0;

        public ReportsPage()
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
                            PaymentId,
                            Adress,
                            Period,
                            Accrued,
                            ISNULL(Paid, 0) as Paid,
                            Owner
                        FROM OtchetPoOplate 
                        ORDER BY Period DESC, PaymentId DESC";

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
                MessageBox.Show($"Ошибка загрузки отчетов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics(DataTable dt)
        {
            int total = dt.Rows.Count;
            int paidCount = 0;
            int debtsCount = 0;
            decimal totalAccrued = 0;
            decimal totalPaid = 0;

            foreach (DataRow row in dt.Rows)
            {
                decimal accrued = row["Accrued"] != DBNull.Value ? Convert.ToDecimal(row["Accrued"]) : 0;
                decimal paid = row["Paid"] != DBNull.Value ? Convert.ToDecimal(row["Paid"]) : 0;

                totalAccrued += accrued;
                totalPaid += paid;

                if (paid >= accrued && accrued > 0) paidCount++;
                if (paid < accrued || paid == 0) debtsCount++;
            }

            txtTotal.Text = $"Всего: {total} записей";
            txtSum.Text = $"Начислено: {totalAccrued:N2} ₽";
            txtPaid.Text = $"Оплачено: {paidCount} (на сумму {totalPaid:N2} ₽)";
            txtDebts.Text = $"Долги: {debtsCount}";
        }

        private void dgReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgReports.SelectedItem != null;
            btnDelete.IsEnabled = hasSelection;
            txtSelected.Text = hasSelection ? "Выбрано: 1" : "Выбрано: 0";

            if (hasSelection && dgReports.SelectedItem is DataRowView row)
            {
                selectedPaymentId = Convert.ToInt32(row["PaymentId"]);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView row)
            {
                int paymentId = Convert.ToInt32(row["PaymentId"]);
                EditReport(paymentId);
            }
        }

        private void EditReport(int paymentId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            PaymentId,
                            Adress,
                            Period,
                            Accrued,
                            ISNULL(Paid, 0) as Paid,
                            Owner
                        FROM OtchetPoOplate 
                        WHERE PaymentId = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", paymentId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Показываем диалоговое окно редактирования
                            ShowEditDialog(
                                paymentId,
                                reader["Adress"].ToString(),
                                reader["Period"].ToString(),
                                Convert.ToDecimal(reader["Accrued"]),
                                Convert.ToDecimal(reader["Paid"]),
                                Convert.ToInt32(reader["Owner"])
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowEditDialog(int paymentId, string address, string period, decimal accrued, decimal paid, int ownerId)
        {
            // Создаем диалоговое окно для редактирования
            Window editDialog = new Window
            {
                Title = "Редактирование отчета по оплате",
                Width = 450,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };

            // Поле для выбора владельца
            TextBlock lblOwner = new TextBlock
            {
                Text = "Владелец:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            ComboBox cmbOwner = new ComboBox
            {
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Загружаем список владельцев
            LoadOwnersToComboBox(cmbOwner);

            // Выбираем текущего владельца
            foreach (ComboBoxItem item in cmbOwner.Items)
            {
                if ((int)item.Tag == ownerId)
                {
                    cmbOwner.SelectedItem = item;
                    break;
                }
            }

            // Поле для адреса
            TextBlock lblAddress = new TextBlock
            {
                Text = "Адрес:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtAddress = new TextBox
            {
                Text = address,
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для периода
            TextBlock lblPeriod = new TextBlock
            {
                Text = "Период:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtPeriod = new TextBox
            {
                Text = period,
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для начисленной суммы
            TextBlock lblAccrued = new TextBlock
            {
                Text = "Начисленная сумма (₽):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtAccrued = new TextBox
            {
                Text = accrued.ToString(),
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для оплаченной суммы
            TextBlock lblPaid = new TextBlock
            {
                Text = "Оплаченная сумма (₽):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtPaid = new TextBox
            {
                Text = paid.ToString(),
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Кнопки
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button btnSave = new Button
            {
                Content = "Сохранить",
                Width = 80,
                Height = 30,
                Background = System.Windows.Media.Brushes.Green,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };

            Button btnCancel = new Button
            {
                Content = "Отмена",
                Width = 80,
                Height = 30,
                IsCancel = true
            };

            btnSave.Click += (s, e) =>
            {
                if (cmbOwner.SelectedItem == null)
                {
                    MessageBox.Show("Выберите владельца", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtAddress.Text))
                {
                    MessageBox.Show("Введите адрес", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPeriod.Text))
                {
                    MessageBox.Show("Введите период", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtAccrued.Text, out decimal accruedValue) || accruedValue < 0)
                {
                    MessageBox.Show("Введите корректную начисленную сумму", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPaid.Text, out decimal paidValue) || paidValue < 0)
                {
                    MessageBox.Show("Введите корректную оплаченную сумму", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ComboBoxItem selectedOwner = (ComboBoxItem)cmbOwner.SelectedItem;
                int selectedOwnerId = (int)selectedOwner.Tag;

                UpdateReport(paymentId, selectedOwnerId, txtAddress.Text.Trim(), txtPeriod.Text.Trim(), accruedValue, paidValue);
                editDialog.DialogResult = true;
            };

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);

            panel.Children.Add(lblOwner);
            panel.Children.Add(cmbOwner);
            panel.Children.Add(lblAddress);
            panel.Children.Add(txtAddress);
            panel.Children.Add(lblPeriod);
            panel.Children.Add(txtPeriod);
            panel.Children.Add(lblAccrued);
            panel.Children.Add(txtAccrued);
            panel.Children.Add(lblPaid);
            panel.Children.Add(txtPaid);
            panel.Children.Add(buttonPanel);

            editDialog.Content = panel;

            if (editDialog.ShowDialog() == true)
            {
                LoadReports(); // Обновляем список
            }
        }

        private void LoadOwnersToComboBox(ComboBox comboBox)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Owner_id, Name FROM Owners ORDER BY Name";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    comboBox.Items.Clear();

                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = $"{reader["Name"]} (ID: {reader["Owner_id"]})",
                            Tag = Convert.ToInt32(reader["Owner_id"])
                        };
                        comboBox.Items.Add(item);
                    }
                    reader.Close();

                    if (comboBox.Items.Count > 0)
                        comboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки владельцев: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateReport(int paymentId, int ownerId, string address, string period, decimal accrued, decimal paid)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        UPDATE OtchetPoOplate SET 
                            Owner = @OwnerId,
                            Adress = @Address,
                            Period = @Period,
                            Accrued = @Accrued,
                            Paid = @Paid
                        WHERE PaymentId = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", paymentId);
                    cmd.Parameters.AddWithValue("@OwnerId", ownerId);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@Period", period);
                    cmd.Parameters.AddWithValue("@Accrued", accrued);
                    cmd.Parameters.AddWithValue("@Paid", paid);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Отчет успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAddDialog()
        {
            Window addDialog = new Window
            {
                Title = "Добавление нового отчета по оплате",
                Width = 450,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };

            // Поле для выбора владельца
            TextBlock lblOwner = new TextBlock
            {
                Text = "Владелец:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            ComboBox cmbOwner = new ComboBox
            {
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Загружаем список владельцев
            LoadOwnersToComboBox(cmbOwner);

            // Поле для адреса
            TextBlock lblAddress = new TextBlock
            {
                Text = "Адрес:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtAddress = new TextBox
            {
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для периода
            TextBlock lblPeriod = new TextBlock
            {
                Text = "Период (например, Март 2025):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtPeriod = new TextBox
            {
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для начисленной суммы
            TextBlock lblAccrued = new TextBlock
            {
                Text = "Начисленная сумма (₽):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtAccrued = new TextBox
            {
                Text = "0",
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для оплаченной суммы
            TextBlock lblPaid = new TextBlock
            {
                Text = "Оплаченная сумма (₽):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtPaid = new TextBox
            {
                Text = "0",
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Кнопки
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button btnSave = new Button
            {
                Content = "Добавить",
                Width = 80,
                Height = 30,
                Background = System.Windows.Media.Brushes.Green,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };

            Button btnCancel = new Button
            {
                Content = "Отмена",
                Width = 80,
                Height = 30,
                IsCancel = true
            };

            btnSave.Click += (s, ev) =>
            {
                if (cmbOwner.SelectedItem == null)
                {
                    MessageBox.Show("Выберите владельца", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtAddress.Text))
                {
                    MessageBox.Show("Введите адрес", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPeriod.Text))
                {
                    MessageBox.Show("Введите период", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtAccrued.Text, out decimal accruedValue) || accruedValue < 0)
                {
                    MessageBox.Show("Введите корректную начисленную сумму", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPaid.Text, out decimal paidValue) || paidValue < 0)
                {
                    MessageBox.Show("Введите корректную оплаченную сумму", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ComboBoxItem selectedOwner = (ComboBoxItem)cmbOwner.SelectedItem;
                int ownerId = (int)selectedOwner.Tag;

                AddNewReport(ownerId, txtAddress.Text.Trim(), txtPeriod.Text.Trim(), accruedValue, paidValue);
                addDialog.DialogResult = true;
            };

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);

            panel.Children.Add(lblOwner);
            panel.Children.Add(cmbOwner);
            panel.Children.Add(lblAddress);
            panel.Children.Add(txtAddress);
            panel.Children.Add(lblPeriod);
            panel.Children.Add(txtPeriod);
            panel.Children.Add(lblAccrued);
            panel.Children.Add(txtAccrued);
            panel.Children.Add(lblPaid);
            panel.Children.Add(txtPaid);
            panel.Children.Add(buttonPanel);

            addDialog.Content = panel;

            if (addDialog.ShowDialog() == true)
            {
                LoadReports(); // Обновляем список
            }
        }

        private void AddNewReport(int ownerId, string address, string period, decimal accrued, decimal paid)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO OtchetPoOplate 
                        (Owner, Adress, Period, Accrued, Paid) 
                        VALUES 
                        (@OwnerId, @Address, @Period, @Accrued, @Paid)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@OwnerId", ownerId);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@Period", period);
                    cmd.Parameters.AddWithValue("@Accrued", accrued);
                    cmd.Parameters.AddWithValue("@Paid", paid);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Отчет успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ShowAddDialog();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPaymentId == 0)
            {
                MessageBox.Show("Выберите отчет для удаления",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранный отчет?",
                "Подтверждение удаления", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM OtchetPoOplate WHERE PaymentId = @Id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", selectedPaymentId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Отчет успешно удален",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadReports();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления отчета: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadReports();
        }
    }
}