using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class ApartPage : Page
    {
        private string connectionString;
        private int selectedApartmentId = 0;

        public ApartPage()
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
            LoadApartmentsData();
        }

        private void LoadApartmentsData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            Apartment_id,
                            Number
                        FROM Apartments
                        ORDER BY Number";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgApartments.ItemsSource = dt.DefaultView;
                    UpdateCounters(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCounters(DataTable dt)
        {
            txtTotal.Text = $"Всего квартир: {dt.Rows.Count}";
        }

        private void dgApartments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgApartments.SelectedItem != null;
            btnDelete.IsEnabled = hasSelection;
            txtSelected.Text = hasSelection ? "Выбрано: 1" : "Выбрано: 0";

            if (hasSelection && dgApartments.SelectedItem is DataRowView row)
            {
                selectedApartmentId = Convert.ToInt32(row["Apartment_id"]);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView row)
            {
                int apartmentId = Convert.ToInt32(row["Apartment_id"]);
                EditApartment(apartmentId);
            }
        }

        private void EditApartment(int apartmentId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Apartments WHERE Apartment_id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", apartmentId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Показываем простое окно редактирования
                            ShowEditDialog(apartmentId, reader["Number"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных квартиры: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowEditDialog(int apartmentId, string currentNumber)
        {
            // Создаем диалоговое окно для редактирования
            Window editDialog = new Window
            {
                Title = "Редактирование квартиры",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };

            // Поле для номера квартиры
            TextBlock lblNumber = new TextBlock
            {
                Text = "Номер квартиры:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtNumber = new TextBox
            {
                Text = currentNumber,
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
                if (string.IsNullOrWhiteSpace(txtNumber.Text))
                {
                    MessageBox.Show("Введите номер квартиры", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtNumber.Text, out int number) || number <= 0)
                {
                    MessageBox.Show("Введите корректный номер квартиры", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                UpdateApartment(apartmentId, number);
                editDialog.DialogResult = true;
            };

            btnCancel.Click += (s, e) =>
            {
                editDialog.DialogResult = false;
            };

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);

            panel.Children.Add(lblNumber);
            panel.Children.Add(txtNumber);
            panel.Children.Add(buttonPanel);

            editDialog.Content = panel;

            if (editDialog.ShowDialog() == true)
            {
                LoadApartmentsData(); // Обновляем список
            }
        }

        private void UpdateApartment(int apartmentId, int number)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Apartments SET Number = @Number WHERE Apartment_id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Number", number);
                    cmd.Parameters.AddWithValue("@Id", apartmentId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Квартира успешно обновлена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления квартиры: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Показываем диалоговое окно для добавления новой квартиры
            Window addDialog = new Window
            {
                Title = "Добавление новой квартиры",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };

            TextBlock lblNumber = new TextBlock
            {
                Text = "Номер квартиры:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtNumber = new TextBox
            {
                FontSize = 14,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 15)
            };

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
                if (string.IsNullOrWhiteSpace(txtNumber.Text))
                {
                    MessageBox.Show("Введите номер квартиры", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtNumber.Text, out int number) || number <= 0)
                {
                    MessageBox.Show("Введите корректный номер квартиры", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AddNewApartment(number);
                addDialog.DialogResult = true;
            };

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);

            panel.Children.Add(lblNumber);
            panel.Children.Add(txtNumber);
            panel.Children.Add(buttonPanel);

            addDialog.Content = panel;

            if (addDialog.ShowDialog() == true)
            {
                LoadApartmentsData(); // Обновляем список
            }
        }

        private void AddNewApartment(int number)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Apartments (Number) VALUES (@Number)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Number", number);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Квартира успешно добавлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления квартиры: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApartmentId == 0)
            {
                MessageBox.Show("Выберите квартиру для удаления",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранную квартиру?",
                "Подтверждение удаления", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Apartments WHERE Apartment_id = @Id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", selectedApartmentId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Квартира успешно удалена",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadApartmentsData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления квартиры: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadApartmentsData();
        }
    }
}