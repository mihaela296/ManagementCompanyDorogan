using System.Windows;
using System.Windows.Controls;

namespace ManagementCompanyDorogan
{
    public static class ValidationHelper
    {
        public static bool ValidateRequired(TextBox textBox, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                ShowError(textBox, $"{fieldName} обязательно для заполнения");
                return false;
            }
            ClearError(textBox);
            return true;
        }

        public static bool ValidateNumeric(TextBox textBox, string fieldName, int minValue = 1)
        {
            if (!int.TryParse(textBox.Text, out int value) || value < minValue)
            {
                ShowError(textBox, $"{fieldName} должно быть числом больше {minValue - 1}");
                return false;
            }
            ClearError(textBox);
            return true;
        }

        public static bool ValidateDecimal(TextBox textBox, string fieldName, decimal minValue = 0)
        {
            if (!decimal.TryParse(textBox.Text, out decimal value) || value < minValue)
            {
                ShowError(textBox, $"{fieldName} должно быть числом больше {minValue}");
                return false;
            }
            ClearError(textBox);
            return true;
        }

        private static void ShowError(Control control, string message)
        {
            control.ToolTip = message;
            control.BorderBrush = System.Windows.Media.Brushes.Red;
            control.Focus();
        }

        private static void ClearError(Control control)
        {
            control.ClearValue(Control.ToolTipProperty);
            control.ClearValue(Control.BorderBrushProperty);
        }
    }

    public static class MessageBoxHelper
    {
        public static void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowWarning(string message, string title = "Предупреждение")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static bool ShowQuestion(string message, string title = "Подтверждение")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}