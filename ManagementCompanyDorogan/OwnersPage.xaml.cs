using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;


namespace ManagementCompanyDorogan
{
    public partial class OwnersPage : Page
    {
        private string connectionString;
        private int selectedOwnerId = 0;

        public OwnersPage()
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
            LoadOwnersData();
        }

        private void LoadOwnersData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Owners ORDER BY Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgOwners.ItemsSource = dt.DefaultView;
                    UpdateCounters(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки владельцев: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCounters(DataTable dt)
        {
            txtTotal.Text = $"Всего владельцев: {dt.Rows.Count}";
        }

        private void dgOwners_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgOwners.SelectedItem != null;
            btnDelete.IsEnabled = hasSelection;
            txtSelected.Text = hasSelection ? "Выбрано: 1" : "Выбрано: 0";

            if (hasSelection && dgOwners.SelectedItem is DataRowView row)
            {
                selectedOwnerId = Convert.ToInt32(row["Owner_id"]);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView row)
            {
                int ownerId = Convert.ToInt32(row["Owner_id"]);
                EditOwner(ownerId);
            }
        }

        private void EditOwner(int ownerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Owners WHERE Owner_id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", ownerId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Показываем диалоговое окно редактирования
                            ShowEditDialog(ownerId, reader["Name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных владельца: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowEditDialog(int ownerId, string currentName)
        {
            // Создаем диалоговое окно для редактирования
            Window editDialog = new Window
            {
                Title = "Редактирование владельца",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };

            // Поле для имени владельца
            TextBlock lblName = new TextBlock
            {
                Text = "Имя владельца:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtName = new TextBox
            {
                Text = currentName,
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
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите имя владельца", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                UpdateOwner(ownerId, txtName.Text.Trim());
                editDialog.DialogResult = true;
            };

            btnCancel.Click += (s, e) =>
            {
                editDialog.DialogResult = false;
            };

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);

            panel.Children.Add(lblName);
            panel.Children.Add(txtName);
            panel.Children.Add(buttonPanel);

            editDialog.Content = panel;

            if (editDialog.ShowDialog() == true)
            {
                LoadOwnersData(); // Обновляем список
            }
        }

        private void UpdateOwner(int ownerId, string name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Owners SET Name = @Name WHERE Owner_id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Id", ownerId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Владелец успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления владельца: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Показываем диалоговое окно для добавления нового владельца
            Window addDialog = new Window
            {
                Title = "Добавление нового владельца",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(15) };

            TextBlock lblName = new TextBlock
            {
                Text = "Имя владельца:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBox txtName = new TextBox
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
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите имя владельца", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AddNewOwner(txtName.Text.Trim());
                addDialog.DialogResult = true;
            };

            buttonPanel.Children.Add(btnSave);
            buttonPanel.Children.Add(btnCancel);

            panel.Children.Add(lblName);
            panel.Children.Add(txtName);
            panel.Children.Add(buttonPanel);

            addDialog.Content = panel;

            if (addDialog.ShowDialog() == true)
            {
                LoadOwnersData(); // Обновляем список
            }
        }

        private void AddNewOwner(string name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Owners (Name) VALUES (@Name)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", name);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Владелец успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления владельца: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOwnerId == 0)
            {
                MessageBox.Show("Выберите владельца для удаления",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранного владельца?\n\n" +
                               "Внимание: Удаление владельца может повлиять на связанные записи в системе.",
                "Подтверждение удаления", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Owners WHERE Owner_id = @Id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", selectedOwnerId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Владелец успешно удален",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadOwnersData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления владельца: {ex.Message}\n\n" +
                                  "Возможно, владелец связан с другими записями в системе.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOwnersData();
        }
    }
}
