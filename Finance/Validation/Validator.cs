using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Finance.Resources;

namespace Finance.Validation
{
    public static class Validator
    {
        private static bool EvaluateExpression(string expression, out double result)
        {
            try
            {
                var parser = new MathParser('.');
                result = parser.Parse(expression);
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
            return true;
        }

        public static bool EvaluateTextBox(TextBox textBox, bool showDialog = false)
        {
            var expression = textBox.Text;
            double result;
            if (EvaluateExpression(expression, out result))
            {
                textBox.Text = result.ToString(CultureInfo.InvariantCulture);
                return true;
            }
            if (showDialog)
            {
                MessageBox.Show(AppResources.AmountIncorrectMessageText, AppResources.AmountIncorrectMessageTitle,
                    MessageBoxButton.OK);
                textBox.Focus();
            }
            return false;
        }

        public static bool EmptyTextBox(TextBox textBox, bool showDialog = false)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (showDialog)
                {
                    MessageBox.Show(AppResources.EmptyField, AppResources.ErrorTitle, MessageBoxButton.OK);
                    textBox.Focus();
                }
                return true;
            }
            return false;
        }
    }
}