using Microsoft.Win32;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;



namespace ManagementCompanyDorogan
{
    public partial class DebtsPage : Page
    {
        private string connectionString;
        private DataTable debtsTable;
        private int selectedDebtId = 0;

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
                            Debt_id,
                            Adress,
                            ISNULL(Apartment, 0) as Apartment,
                            ISNULL(Water, 0) as Water,
                            ISNULL(Electricity, 0) as Electricity,
                            ISNULL(Phone, '') as Phone
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
                decimal water = row["Water"] != DBNull.Value ? Convert.ToDecimal(row["Water"]) : 0;
                decimal electricity = row["Electricity"] != DBNull.Value ? Convert.ToDecimal(row["Electricity"]) : 0;

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

        private void dgDebts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgDebts.SelectedItem != null;
            btnDelete.IsEnabled = hasSelection;
            txtSelected.Text = hasSelection ? "Выбрано: 1" : "Выбрано: 0";

            if (hasSelection && dgDebts.SelectedItem is DataRowView row)
            {
                selectedDebtId = Convert.ToInt32(row["Debt_id"]);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView row)
            {
                int debtId = Convert.ToInt32(row["Debt_id"]);
                EditDebt(debtId);
            }
        }

        private void EditDebt(int debtId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            Debt_id,
                            Adress,
                            ISNULL(Apartment, 0) as Apartment,
                            ISNULL(Water, 0) as Water,
                            ISNULL(Electricity, 0) as Electricity,
                            ISNULL(Phone, '') as Phone
                        FROM Debt 
                        WHERE Debt_id = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", debtId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Показываем диалоговое окно редактирования
                            ShowEditDialog(
                                debtId,
                                reader["Adress"].ToString(),
                                Convert.ToInt32(reader["Apartment"]),
                                Convert.ToDecimal(reader["Water"]),
                                Convert.ToDecimal(reader["Electricity"]),
                                reader["Phone"].ToString()
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных долга: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowEditDialog(int debtId, string address, int apartment, decimal water, decimal electricity, string phone)
        {
            // Создаем диалоговое окно для редактирования
            Window editDialog = new Window
            {
                Title = "Редактирование задолженности",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };

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

            // Поле для номера квартиры
            TextBlock lblApartment = new TextBlock
            {
                Text = "Номер квартиры:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtApartment = new TextBox
            {
                Text = apartment.ToString(),
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для задолженности по воде
            TextBlock lblWater = new TextBlock
            {
                Text = "Задолженность по воде (₽):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtWater = new TextBox
            {
                Text = water.ToString(),
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для задолженности по электричеству
            TextBlock lblElectricity = new TextBlock
            {
                Text = "Задолженность по электричеству (₽):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtElectricity = new TextBox
            {
                Text = electricity.ToString(),
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для телефона
            TextBlock lblPhone = new TextBlock
            {
                Text = "Телефон:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtPhone = new TextBox
            {
                Text = phone,
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
                if (string.IsNullOrWhiteSpace(txtAddress.Text))
                {
                    MessageBox.Show("Введите адрес", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtApartment.Text, out int apartmentValue) || apartmentValue < 0)
                {
                    MessageBox.Show("Введите корректный номер квартиры", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtWater.Text, out decimal waterValue) || waterValue < 0)
                {
                    MessageBox.Show("Введите корректную сумму задолженности по воде", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtElectricity.Text, out decimal electricityValue) || electricityValue < 0)
                {
                    MessageBox.Show("Введите корректную сумму задолженности по электричеству", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                UpdateDebt(debtId, txtAddress.Text.Trim(), apartmentValue, waterValue, electricityValue, txtPhone.Text.Trim());
                editDialog.DialogResult = true;
            };

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);

            panel.Children.Add(lblAddress);
            panel.Children.Add(txtAddress);
            panel.Children.Add(lblApartment);
            panel.Children.Add(txtApartment);
            panel.Children.Add(lblWater);
            panel.Children.Add(txtWater);
            panel.Children.Add(lblElectricity);
            panel.Children.Add(txtElectricity);
            panel.Children.Add(lblPhone);
            panel.Children.Add(txtPhone);
            panel.Children.Add(buttonPanel);

            editDialog.Content = panel;

            if (editDialog.ShowDialog() == true)
            {
                LoadDebts(); // Обновляем список
            }
        }

        private void UpdateDebt(int debtId, string address, int apartment, decimal water, decimal electricity, string phone)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        UPDATE Debt SET 
                            Adress = @Address,
                            Apartment = @Apartment,
                            Water = @Water,
                            Electricity = @Electricity,
                            Phone = @Phone
                        WHERE Debt_id = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", debtId);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@Apartment", apartment == 0 ? (object)DBNull.Value : apartment);
                    cmd.Parameters.AddWithValue("@Water", water);
                    cmd.Parameters.AddWithValue("@Electricity", electricity);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Задолженность успешно обновлена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления задолженности: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Показываем диалоговое окно для добавления новой задолженности
            Window addDialog = new Window
            {
                Title = "Добавление новой задолженности",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };

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

            // Поле для номера квартиры
            TextBlock lblApartment = new TextBlock
            {
                Text = "Номер квартиры (необязательно):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtApartment = new TextBox
            {
                Text = "0",
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для задолженности по воде
            TextBlock lblWater = new TextBlock
            {
                Text = "Задолженность по воде (₽):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtWater = new TextBox
            {
                Text = "0",
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для задолженности по электричеству
            TextBlock lblElectricity = new TextBlock
            {
                Text = "Задолженность по электричеству (₽):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtElectricity = new TextBox
            {
                Text = "0",
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Поле для телефона
            TextBlock lblPhone = new TextBlock
            {
                Text = "Телефон (необязательно):",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtPhone = new TextBox
            {
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
                if (string.IsNullOrWhiteSpace(txtAddress.Text))
                {
                    MessageBox.Show("Введите адрес", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtApartment.Text, out int apartmentValue) || apartmentValue < 0)
                {
                    MessageBox.Show("Введите корректный номер квартиры", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtWater.Text, out decimal waterValue) || waterValue < 0)
                {
                    MessageBox.Show("Введите корректную сумму задолженности по воде", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtElectricity.Text, out decimal electricityValue) || electricityValue < 0)
                {
                    MessageBox.Show("Введите корректную сумму задолженности по электричеству", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AddNewDebt(txtAddress.Text.Trim(), apartmentValue, waterValue, electricityValue, txtPhone.Text.Trim());
                addDialog.DialogResult = true;
            };

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);

            panel.Children.Add(lblAddress);
            panel.Children.Add(txtAddress);
            panel.Children.Add(lblApartment);
            panel.Children.Add(txtApartment);
            panel.Children.Add(lblWater);
            panel.Children.Add(txtWater);
            panel.Children.Add(lblElectricity);
            panel.Children.Add(txtElectricity);
            panel.Children.Add(lblPhone);
            panel.Children.Add(txtPhone);
            panel.Children.Add(buttonPanel);

            addDialog.Content = panel;

            if (addDialog.ShowDialog() == true)
            {
                LoadDebts(); // Обновляем список
            }
        }

        private void AddNewDebt(string address, int apartment, decimal water, decimal electricity, string phone)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO Debt 
                        (Adress, Apartment, Water, Electricity, Phone) 
                        VALUES 
                        (@Address, @Apartment, @Water, @Electricity, @Phone)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@Apartment", apartment == 0 ? (object)DBNull.Value : apartment);
                    cmd.Parameters.AddWithValue("@Water", water);
                    cmd.Parameters.AddWithValue("@Electricity", electricity);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Задолженность успешно добавлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления задолженности: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDebtId == 0)
            {
                MessageBox.Show("Выберите задолженность для удаления",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранную задолженность?",
                "Подтверждение удаления", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Debt WHERE Debt_id = @Id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", selectedDebtId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Задолженность успешно удалена",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadDebts();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления задолженности: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                            writer.WriteLine($"Адрес: {row["Adress"]}");
                            writer.WriteLine($"Квартира: {row["Apartment"]}");
                            writer.WriteLine($"Вода: {row["Water"]} ₽");
                            writer.WriteLine($"Электричество: {row["Electricity"]} ₽");

                            decimal water = row["Water"] != DBNull.Value ? Convert.ToDecimal(row["Water"]) : 0;
                            decimal electricity = row["Electricity"] != DBNull.Value ? Convert.ToDecimal(row["Electricity"]) : 0;
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
