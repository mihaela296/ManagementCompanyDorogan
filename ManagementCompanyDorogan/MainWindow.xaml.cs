using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace ManagementCompanyDorogan
{
    public partial class MainWindow : Window
    {
        private SqlConnection connection;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Пробуем получить строку подключения разными способами
                string connectionString = GetConnectionString();

                if (string.IsNullOrEmpty(connectionString))
                {
                    // Если не нашли строку подключения
                    StatusText.Text = "Не настроено подключение к БД";
                    MessageBox.Show("Не настроено подключение к базе данных.\n\n" +
                                   "Приложение запустится в оффлайн-режиме.",
                                   "Предупреждение",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Все равно загружаем главную страницу
                    MainFrame.Navigate(new MainPage());
                    return;
                }

                // Создаем подключение
                connection = new SqlConnection(connectionString);

                // Пытаемся подключиться
                connection.Open();
                StatusText.Text = "Подключено к БД";

                // Загружаем главную страницу
                MainFrame.Navigate(new MainPage());
            }
            catch (SqlException ex)
            {
                StatusText.Text = "Ошибка подключения к БД";
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}\n\n" +
                               "Приложение запустится в оффлайн-режиме.",
                               "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);

                // Все равно загружаем главную страницу
                MainFrame.Navigate(new MainPage());
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка загрузки";
                MessageBox.Show($"Ошибка при запуске приложения:\n{ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);

                // Все равно загружаем главную страницу
                MainFrame.Navigate(new MainPage());
            }
        }

        private string GetConnectionString()
        {
            // Способ 1: Прямая строка подключения (надежнее всего)
            string directConnectionString =
                "Data Source=ManagementCompany.mssql.somee.com;" +
                "Initial Catalog=ManagementCompany;" +
                "User ID=Dorogan_SQLLogin_1;" +
                "Password=x4c9e1mit9;" +
                "TrustServerCertificate=True;" +
                "MultipleActiveResultSets=True";

            try
            {
                // Способ 2: Из существующей строки Entities
                var entityConnection = ConfigurationManager.ConnectionStrings["Entities"];
                if (entityConnection != null && !string.IsNullOrEmpty(entityConnection.ConnectionString))
                {
                    // Извлекаем обычную строку из Entity Framework строки
                    string entityString = entityConnection.ConnectionString;

                    // Ищем provider connection string
                    string searchStr = "provider connection string=\"";
                    int startIndex = entityString.IndexOf(searchStr);

                    if (startIndex >= 0)
                    {
                        startIndex += searchStr.Length;
                        int endIndex = entityString.IndexOf("\"", startIndex);

                        if (endIndex > startIndex)
                        {
                            string extracted = entityString.Substring(startIndex, endIndex - startIndex);
                            // Заменяем HTML-сущности
                            extracted = extracted.Replace("&quot;", "\"");
                            return extracted;
                        }
                    }
                }

                // Способ 3: Из ManagementCompanyDB (если добавили в App.config)
                var managementDB = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"];
                if (managementDB != null && !string.IsNullOrEmpty(managementDB.ConnectionString))
                {
                    return managementDB.ConnectionString;
                }

                // Если ничего не нашли, используем прямую строку
                return directConnectionString;
            }
            catch
            {
                // Если произошла ошибка, используем прямую строку
                return directConnectionString;
            }
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MainPage());
        }

        private void RequestsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RequestsPage());
        }

        private void AddRequestButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new EditRequestPage(null));
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RequestHistoryPage());
        }

        private void HousingButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HousingFundPage());
        }

        private void OwnersButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OwnersPage());
        }

        private void PaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PaymentsPage());
        }

        private void DebtsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DebtsPage());
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти из приложения?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}