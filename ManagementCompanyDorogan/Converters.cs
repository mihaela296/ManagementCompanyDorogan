using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Data;

namespace ManagementCompanyDorogan
{
    // Конвертер ID собственника в имя
    public class OwnerIdToNameConverter : IValueConverter
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DBNull.Value || string.IsNullOrEmpty(connectionString))
                return "Не указан";

            try
            {
                if (!int.TryParse(value.ToString(), out int ownerId))
                    return "Не указан";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Name FROM Owners WHERE Owner_id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", ownerId);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "Не найден";
                }
            }
            catch
            {
                return "Ошибка";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертер ID квартиры в номер
    public class ApartmentIdToNumberConverter : IValueConverter
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DBNull.Value || string.IsNullOrEmpty(connectionString))
                return "-";

            try
            {
                if (!int.TryParse(value.ToString(), out int apartmentId))
                    return "-";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Number FROM Apartments WHERE Apartment_id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", apartmentId);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "-";
                }
            }
            catch
            {
                return "-";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертер кода оплаты в статус заявки
    public class PaymentToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
                return "Нет данных";

            try
            {
                decimal paid = System.Convert.ToDecimal(value);
                if (paid == 0) return "Открыта";
                if (paid == 1) return "В работе";
                if (paid == 2) return "Закрыта";
                return "Оплата";
            }
            catch
            {
                return "Ошибка";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертер статуса в цвет
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
                return System.Windows.Media.Brushes.Black;

            try
            {
                decimal status = System.Convert.ToDecimal(value);
                if (status == 0) return System.Windows.Media.Brushes.Red;        // Открыта
                if (status == 1) return System.Windows.Media.Brushes.Orange;     // В работе
                if (status == 2) return System.Windows.Media.Brushes.Green;      // Закрыта
                return System.Windows.Media.Brushes.Blue;                        // Оплата
            }
            catch
            {
                return System.Windows.Media.Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертер числа в дату (Accrued → Дата)
    public class NumberToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
                return "";

            try
            {
                decimal number = System.Convert.ToDecimal(value);
                // Предполагаем, что Accrued хранится как YYYYMMDD
                string str = number.ToString("0");
                if (str.Length >= 8)
                {
                    string year = str.Substring(0, 4);
                    string month = str.Substring(4, 2);
                    string day = str.Substring(6, 2);
                    return $"{day}.{month}.{year}";
                }
                return str;
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертер года постройки в возраст
    public class YearToAgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DBNull.Value) return "-";

            if (int.TryParse(value.ToString(), out int year) && year > 0)
            {
                int age = DateTime.Now.Year - year;
                return age > 0 ? age.ToString() : "Новый";
            }

            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертер для OwnersPage
    public class OwnerToApartmentsConverter : IValueConverter
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DBNull.Value || string.IsNullOrEmpty(connectionString))
                return "Нет данных";

            try
            {
                if (!int.TryParse(value.ToString(), out int ownerId))
                    return "Нет данных";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Найти квартиры собственника через таблицу Debt
                    string query = @"
                        SELECT COUNT(DISTINCT Apartment) 
                        FROM Debt 
                        WHERE Owner = @OwnerId AND Apartment IS NOT NULL";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@OwnerId", ownerId);
                    var result = cmd.ExecuteScalar();

                    int count = result != DBNull.Value ? System.Convert.ToInt32(result) : 0;
                    return count > 0 ? $"{count} кв." : "Нет";
                }
            }
            catch
            {
                return "Ошибка";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OwnerToDebtConverter : IValueConverter
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DBNull.Value || string.IsNullOrEmpty(connectionString))
                return "Нет данных";

            try
            {
                if (!int.TryParse(value.ToString(), out int ownerId))
                    return "Нет данных";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT SUM(ISNULL(Water, 0) + ISNULL(Electricity, 0)) 
                        FROM Debt 
                        WHERE Owner = @OwnerId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@OwnerId", ownerId);
                    var result = cmd.ExecuteScalar();

                    if (result != DBNull.Value && result != null)
                    {
                        decimal debt = System.Convert.ToDecimal(result);
                        return debt > 0 ? $"{debt:N2} ₽" : "Нет долга";
                    }
                    return "Нет данных";
                }
            }
            catch
            {
                return "Ошибка";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OwnerToLastPaymentConverter : IValueConverter
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["ManagementCompanyDB"]?.ConnectionString;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DBNull.Value || string.IsNullOrEmpty(connectionString))
                return "Нет данных";

            try
            {
                if (!int.TryParse(value.ToString(), out int ownerId))
                    return "Нет данных";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT TOP 1 Period 
                        FROM OtchetPoOplate 
                        WHERE Owner = @OwnerId AND Paid > 0 
                        ORDER BY Accrued DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@OwnerId", ownerId);
                    var result = cmd.ExecuteScalar();

                    return result?.ToString() ?? "Нет платежей";
                }
            }
            catch
            {
                return "Ошибка";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертеры для DebtsPage
    public class DebtTotalConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length < 2) return "0 ₽";

            try
            {
                decimal water = values[0] != DBNull.Value && values[0] != null ?
                    System.Convert.ToDecimal(values[0]) : 0;
                decimal electricity = values[1] != DBNull.Value && values[1] != null ?
                    System.Convert.ToDecimal(values[1]) : 0;

                return $"{(water + electricity):N2} ₽";
            }
            catch
            {
                return "Ошибка";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DebtAmountToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return System.Windows.Media.Brushes.Black;

            try
            {
                decimal water = values[0] != DBNull.Value && values[0] != null ?
                    System.Convert.ToDecimal(values[0]) : 0;
                decimal electricity = values[1] != DBNull.Value && values[1] != null ?
                    System.Convert.ToDecimal(values[1]) : 0;

                decimal total = water + electricity;

                if (total > 10000)
                    return System.Windows.Media.Brushes.Red;
                else if (total > 1000)
                    return System.Windows.Media.Brushes.Orange;
                else if (total > 0)
                    return System.Windows.Media.Brushes.Black;
                else
                    return System.Windows.Media.Brushes.Green;
            }
            catch
            {
                return System.Windows.Media.Brushes.Black;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертеры для PaymentsPage
    public class PaymentBalanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Упрощенная версия
            return "0 ₽";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BalanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Упрощенная версия
            return System.Windows.Media.Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PaymentToStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Упрощенная версия
            return "Нет данных";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}