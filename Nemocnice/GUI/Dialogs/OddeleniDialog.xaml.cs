using Nemocnice.Database;
using Nemocnice.Model;
using Nemocnice.ModelObjects;
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
    /// Interakční logika pro OddeleniDialog.xaml
    /// </summary>
    public partial class OddeleniDialog : Window
    {
        public OracleConnection Connection { get; }

        public OddeleniDialog()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DatabaseConnection db = DatabaseConnection.Instance;
            Connection = db.OracleConnection;
            InitializeComponent();
            InitComboBoxes();
        }
        private void InitComboBoxes()
        {
            try // budovy cb fill
            {
                BudovaCb.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisBudovy", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int? pocetPater = reader.IsDBNull(reader.GetOrdinal("pocet_pater")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("pocet_pater"));
                            Budova budova = new Budova(
                                reader.GetInt32("id_budova"),
                                reader.GetString("nazev"),
                                pocetPater,
                                reader.GetInt32("id_adresa"));
                            BudovaCb.Items.Add(budova);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování Kategorie: " + ex.Message);
            }
            BudovaCb.SelectedIndex = 0;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tabulka = "oddeleni";
                string sloupce = "nazev, id_budova";
                Budova? budova = BudovaCb.SelectedItem as Budova;
                string hodnoty = $"'{NazevTb.Text}', {budova.Id}";

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
