using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class SpisokPage : Page
    {
        private string connectionString;
        private int selectedFondId = 0;

        public SpisokPage()
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
            LoadHousingData();
        }

        private void LoadHousingData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            Fond_id,
                            Adress,
                            StartDate,
                            ISNULL(Floors, 0) as Floors,
                            ISNULL(Apartament, 0) as Apartament,
                            ISNULL(Year, 0) as Year,
                            ISNULL(Square, 0) as Square
                        FROM SpisokJilogoFonda
                        ORDER BY Adress";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgHousing.ItemsSource = dt.DefaultView;
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
            txtTotal.Text = $"Всего записей: {dt.Rows.Count}";
        }

        private void dgHousing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgHousing.SelectedItem != null;
            btnDelete.IsEnabled = hasSelection;
            txtSelected.Text = hasSelection ? "Выбрано: 1" : "Выбрано: 0";

            if (hasSelection && dgHousing.SelectedItem is DataRowView row)
            {
                selectedFondId = Convert.ToInt32(row["Fond_id"]);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView row)
            {
                int fondId = Convert.ToInt32(row["Fond_id"]);
                EditRecord(fondId);
            }
        }

        private void EditRecord(int fondId)
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
                            // Переходим на страницу редактирования
                            NavigationService.Navigate(new AddEditSpisokPage(fondId, true));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записи: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditSpisokPage(0, false));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFondId == 0)
            {
                MessageBox.Show("Выберите запись для удаления",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?",
                "Подтверждение удаления", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM SpisokJilogoFonda WHERE Fond_id = @Id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", selectedFondId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Запись успешно удалена",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadHousingData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления записи: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadHousingData();
        }
    }
}