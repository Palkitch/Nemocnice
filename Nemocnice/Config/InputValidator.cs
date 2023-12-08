using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Nemocnice.Config
{
    public class InputValidator
    {
        public static bool ParsableIntFromTextBox(TextBox input, string label, out int result)
        {
            result = 0;

            try
            {
                result = int.Parse(input.Text);
                return true;
            }
            catch (Exception)
            {
                ShowInvalidFormatMessageBox(input, label);
                return false;
            }
        }

        private static void ShowInvalidFormatMessageBox(TextBox input, string label)
        {
            MessageBox.Show($"Špatná hodnota '{input.Text}' pro {label}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
