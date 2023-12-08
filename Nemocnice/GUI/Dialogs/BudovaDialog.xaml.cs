using Nemocnice.Config;
using Nemocnice.Database;
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
    /// Interakční logika pro BudovaDialog.xaml
    /// </summary>
    public partial class BudovaDialog : Window
    {
        public OracleConnection Connection { get; }

        public BudovaDialog()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DatabaseConnection db = DatabaseConnection.Instance;
            Connection = db.OracleConnection;
            InitializeComponent();
            InitComboBoxes();
        }

        private void InitComboBoxes()
        {
            try // adresa cb fill
            {
                AdresaCb.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisAdresy", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Adresa adresa = new Adresa(
                                Convert.ToInt32(reader["ID_ADRESA"]),
                                Convert.ToInt32(reader["CISLO_POPISNE"]),
                                reader.GetString("ulice"),
                                reader.GetString("MESTO"),
                                reader.GetString("STAT"),
                                Convert.ToInt32(reader["PSC"])
                            );
                            AdresaCb.Items.Add(adresa);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování Adresy: " + ex.Message);
            }
            AdresaCb.SelectedIndex = 0;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            int pocetPater;
            if (InputValidator.ParsableIntFromTextBox(PocetPaterTb, "Počet pater", out pocetPater))
            try
            {
                string tabulka = "budovy";
                string sloupce = "nazev, pocet_pater, id_adresa";
                Adresa? adresa = AdresaCb.SelectedItem as Adresa;
                string hodnoty = $"'{NazevTb.Text}', {pocetPater}, {adresa.Id}";

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
