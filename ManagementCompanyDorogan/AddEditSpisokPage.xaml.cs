using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class AddEditSpisokPage : Page
    {
        private string connectionString;
        private int fondId;
        private bool isEditMode;

        public AddEditSpisokPage(int id, bool editMode)
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;
            fondId = id;
            isEditMode = editMode;

            if (isEditMode)
            {
                txtTitle.Text = "Редактирование жилого фонда";
                LoadRecordData();
            }
        }

        private void LoadRecordData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM SpisokJilogoFonda WHERE Fond_id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", fondId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtAddress.Text = reader["Adress"]?.ToString() ?? "";

                            if (reader["StartDate"] != DBNull.Value)
                            {
                                dpStartDate.SelectedDate = Convert.ToDateTime(reader["StartDate"]);
                            }

                            txtFloors.Text = reader["Floors"]?.ToString() ?? "";
                            txtApartments.Text = reader["Apartament"]?.ToString() ?? "";
                            txtYear.Text = reader["Year"]?.ToString() ?? "";
                            txtSquare.Text = reader["Square"]?.ToString() ?? "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                ShowError("Введите адрес здания");
                txtAddress.Focus();
                return;
            }

            if (dpStartDate.SelectedDate == null)
            {
                ShowError("Выберите дату начала управления");
                dpStartDate.Focus();
                return;
            }

            if (!int.TryParse(txtFloors.Text, out int floors) || floors <= 0)
            {
                ShowError("Введите корректное количество этажей");
                txtFloors.Focus();
                return;
            }

            if (!int.TryParse(txtYear.Text, out int year) || year <= 0)
            {
                ShowError("Введите корректный год постройки");
                txtYear.Focus();
                return;
            }

            if (!decimal.TryParse(txtSquare.Text, out decimal square) || square <= 0)
            {
                ShowError("Введите корректную площадь");
                txtSquare.Focus();
                return;
            }

            // Парсим квартиры (необязательное поле)
            int apartments = 0;
            if (!string.IsNullOrWhiteSpace(txtApartments.Text) &&
                !int.TryParse(txtApartments.Text, out apartments))
            {
                ShowError("Введите корректное количество квартир");
                txtApartments.Focus();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    if (isEditMode)
                    {
                        // Обновление записи
                        string query = @"
                            UPDATE SpisokJilogoFonda SET
                                Adress = @Address,
                                StartDate = @StartDate,
                                Floors = @Floors,
                                Apartament = @Apartments,
                                Year = @Year,
                                Square = @Square
                            WHERE Fond_id = @Id";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        AddParameters(cmd);
                        cmd.Parameters.AddWithValue("@Id", fondId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные обновлены успешно!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            NavigationService?.GoBack();
                        }
                    }
                    else
                    {
                        // Добавление новой записи
                        string query = @"
                            INSERT INTO SpisokJilogoFonda 
                            (Adress, StartDate, Floors, Apartament, Year, Square) 
                            VALUES 
                            (@Address, @StartDate, @Floors, @Apartments, @Year, @Square)";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        AddParameters(cmd);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Данные сохранены успешно!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void AddParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
            cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate);
            cmd.Parameters.AddWithValue("@Floors", int.Parse(txtFloors.Text));

            if (!string.IsNullOrWhiteSpace(txtApartments.Text) && int.TryParse(txtApartments.Text, out int apartments))
            {
                cmd.Parameters.AddWithValue("@Apartments", apartments);
            }
            else
            {
                cmd.Parameters.AddWithValue("@Apartments", DBNull.Value);
            }

            cmd.Parameters.AddWithValue("@Year", int.Parse(txtYear.Text));
            cmd.Parameters.AddWithValue("@Square", decimal.Parse(txtSquare.Text));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            errorBorder.Visibility = Visibility.Collapsed;
        }
    }
}