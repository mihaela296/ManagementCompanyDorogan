using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ManagementCompanyDorogan
{
    public partial class OwnersPage : Page
    {
        private string connectionString;
        private DataTable ownersTable;

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
            LoadOwners();
        }

        private void LoadOwners()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Owners ORDER BY Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    ownersTable = new DataTable();
                    adapter.Fill(ownersTable);

                    dgOwners.ItemsSource = ownersTable.DefaultView;

                    // Обновляем заголовок с количеством
                    txtTitle.Text = $"Собственники ({ownersTable.Rows.Count} записей)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки собственников:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtTitle.Text = "Ошибка загрузки собственников";
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOwners();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // Простой диалог поиска
            Window dialog = new Window
            {
                Title = "Поиск собственника",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };

            panel.Children.Add(new TextBlock
            {
                Text = "Введите ФИО или часть:",
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
                Content = "Поиск",
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
                string searchText = textBox.Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    DataView view = ownersTable.DefaultView;
                    view.RowFilter = $"Name LIKE '%{searchText.Replace("'", "''")}%'";
                    txtTitle.Text = $"Собственники (найдено {view.Count} из {ownersTable.Rows.Count})";
                }
                else
                {
                    ownersTable.DefaultView.RowFilter = "";
                    txtTitle.Text = $"Собственники ({ownersTable.Rows.Count} записей)";
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
    }
}