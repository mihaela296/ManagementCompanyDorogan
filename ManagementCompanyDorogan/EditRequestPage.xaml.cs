using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.Collections.Generic;

namespace ManagementCompanyDorogan
{
    public partial class EditRequestPage : Page
    {
        private string connectionString;
        private int? paymentId;

        public EditRequestPage(int? id)
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;
            paymentId = id;

            if (paymentId.HasValue)
                this.Title = "Редактирование заявки";
            else
                this.Title = "Новая заявка";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем список собственников при загрузке страницы
            LoadOwners();

            // Если это редактирование существующей заявки - загружаем данные
            if (paymentId.HasValue)
            {
                LoadRequestData();
            }
            else
            {
                // Устанавливаем значения по умолчанию для новой заявки
                cmbStatus.SelectedIndex = 0; // "Открыта заявка"
            }
        }

        private void LoadOwners()
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    MessageBox.Show("Не настроено подключение к БД", "Ошибка");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Получаем ID и имя собственника
                    string query = "SELECT Owner_id, Name FROM Owners ORDER BY Name";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Очищаем ComboBox
                    cmbOwner.Items.Clear();

                    // Добавляем элемент по умолчанию
                    cmbOwner.Items.Add(new ComboBoxItem
                    {
                        Content = "Выберите собственника...",
                        Tag = -1
                    });

                    // Добавляем всех собственников из БД
                    while (reader.Read())
                    {
                        int ownerId = Convert.ToInt32(reader["Owner_id"]);
                        string ownerName = reader["Name"].ToString();

                        if (!string.IsNullOrWhiteSpace(ownerName))
                        {
                            cmbOwner.Items.Add(new ComboBoxItem
                            {
                                Content = ownerName,
                                Tag = ownerId
                            });
                        }
                    }
                    reader.Close();

                    // Устанавливаем первый элемент как выбранный
                    if (cmbOwner.Items.Count > 0)
                        cmbOwner.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка собственников:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRequestData()
        {
            try
            {
                if (!paymentId.HasValue) return;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT 
                    o.Adress, 
                    o.Period, 
                    o.Paid, 
                    o.Owner as OwnerId,
                    ow.Name as OwnerName
                FROM OtchetPoOplate o
                LEFT JOIN Owners ow ON o.Owner = ow.Owner_id
                WHERE o.PaymentId = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", paymentId.Value);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Заполняем поля данными из БД
                        txtAddress.Text = reader["Adress"]?.ToString() ?? "";
                        txtDescription.Text = reader["Period"]?.ToString() ?? "";

                        // Устанавливаем статус
                        if (reader["Paid"] != DBNull.Value)
                        {
                            decimal status = Convert.ToDecimal(reader["Paid"]);
                            if (status == 1) cmbStatus.SelectedIndex = 1;
                            else if (status == 2) cmbStatus.SelectedIndex = 2;
                            else cmbStatus.SelectedIndex = 0;
                        }

                        // Устанавливаем собственника
                        if (reader["OwnerId"] != DBNull.Value)
                        {
                            int ownerId = Convert.ToInt32(reader["OwnerId"]);

                            // Ищем собственника в списке по ID
                            foreach (ComboBoxItem item in cmbOwner.Items)
                            {
                                if (item.Tag is int itemOwnerId && itemOwnerId == ownerId)
                                {
                                    cmbOwner.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных заявки:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int? GetSelectedOwnerId()
        {
            if (cmbOwner.SelectedItem is ComboBoxItem selectedItem)
            {
                // Проверяем, не выбран ли элемент по умолчанию
                string content = selectedItem.Content.ToString();
                if (content == "Выберите собственника..." || content == "-- Ввести вручную --")
                {
                    return null;
                }

                // Возвращаем ID собственника из свойства Tag
                return selectedItem.Tag as int?;
            }

            return null; // Если ничего не выбрано
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем обязательные поля
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                ShowError("Введите адрес");
                txtAddress.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                ShowError("Введите описание проблемы");
                txtDescription.Focus();
                return;
            }

            int? ownerId = GetSelectedOwnerId();
            if (!ownerId.HasValue || ownerId.Value <= 0)
            {
                ShowError("Выберите собственника");
                cmbOwner.Focus();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Преобразуем статус в число
                    string statusText = ((ComboBoxItem)cmbStatus.SelectedItem)?.Content.ToString();
                    decimal status = 0;
                    if (statusText == "Заявка в работе") status = 1;
                    else if (statusText == "Заявка закрыта") status = 2;

                    if (paymentId.HasValue)
                    {
                        // Обновление существующей заявки
                        string query = @"
                    UPDATE OtchetPoOplate SET 
                        Adress = @Address,
                        Period = @Period,
                        Paid = @Paid,
                        Owner = @OwnerId
                    WHERE PaymentId = @Id";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", paymentId.Value);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                        cmd.Parameters.AddWithValue("@Period", txtDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@Paid", status);
                        cmd.Parameters.AddWithValue("@OwnerId", ownerId.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Заявка обновлена", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            NavigationService.GoBack();
                        }
                    }
                    else
                    {
                        // Добавление новой заявки
                        string query = @"
                    INSERT INTO OtchetPoOplate 
                    (Adress, Period, Accrued, Paid, Owner) 
                    VALUES 
                    (@Address, @Period, @Accrued, @Paid, @OwnerId)";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                        cmd.Parameters.AddWithValue("@Period", txtDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@Accrued", DateTime.Now.ToString("yyyyMMdd"));
                        cmd.Parameters.AddWithValue("@Paid", status);
                        cmd.Parameters.AddWithValue("@OwnerId", ownerId.Value);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Заявка сохранена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            txtMessage.Text = message;
            txtMessage.Visibility = Visibility.Visible;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}