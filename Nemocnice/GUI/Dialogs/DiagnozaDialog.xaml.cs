using Nemocnice.Database;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nemocnice.GUI.Dialogs
{
    /// <summary>
    /// Interakční logika pro DiagnozaDialog.xaml
    /// </summary>
    public partial class DiagnozaDialog : Window
    {
        public OracleConnection Connection { get; }

        public DiagnozaDialog()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DatabaseConnection db = DatabaseConnection.Instance;
            Connection = db.OracleConnection;
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tabulka = "diagnozy_ciselnik";
                string sloupce = "nazev";
                string hodnoty = $"'{NazevTb.Text}'";

                using (OracleCommand cmd = new OracleCommand("insert_data", Connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new OracleParameter("p_tabulka", tabulka));
                    cmd.Parameters.Add(new OracleParameter("p_sloupce", sloupce));
                    cmd.Parameters.Add(new OracleParameter("p_hodnoty", hodnoty));

                    cmd.ExecuteNonQuery();

                    // Data byla úspěšně vložena
                    MessageBox.Show("Data byla úspěšně vložena do tabulky " + tabulka, "Úspěch", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // Chyba při vkládání dat
                MessageBox.Show("Chyba při vkládání dat: " + ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Zavřít okno
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
