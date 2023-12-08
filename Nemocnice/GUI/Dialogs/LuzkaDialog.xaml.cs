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
    /// Interakční logika pro LuzkaDialog.xaml
    /// </summary>
    public partial class LuzkaDialog : Window
    {
        public OracleConnection Connection { get; }

        public LuzkaDialog()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DatabaseConnection db = DatabaseConnection.Instance;
            Connection = db.OracleConnection;
            InitializeComponent();
            InitComboBoxes();
        }

        private void InitComboBoxes()
        {
            try // sestra cb fill
            {
                SestraCb.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisSestry", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Zamestnanec sestra = new Zamestnanec(
                                Convert.ToInt32(reader["id_zamestnanec"]),
                                reader.GetString("jmeno"),
                                reader.GetString("prijmeni")
                            );
                            SestraCb.Items.Add(sestra);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování Sestry: " + ex.Message);
            }
            SestraCb.SelectedIndex = 0;


            try // pokoje cb fill
            {
                PokojeCb.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisPokoje", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            Pokoj pokoj = new Pokoj(
                                Convert.ToInt32(reader["id_pokoj"]),
                                Convert.ToInt32(reader["cislo_pokoje"]),
                                Convert.ToInt32(reader["id_oddeleni"])
                            );
                            PokojeCb.Items.Add(pokoj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování Pokoje: " + ex.Message);
            }
            PokojeCb.SelectedIndex = 0;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            int cislo;
            if (InputValidator.ParsableIntFromTextBox(CisloTb, "Číslo", out cislo))
            try
            {
                string tabulka = "luzka";
                string sloupce = "cislo, sestra_id_zamestnanec, id_pokoj";
                Zamestnanec? zamestnanec = SestraCb.SelectedItem as Zamestnanec;
                Pokoj? pokoj = PokojeCb.SelectedItem as Pokoj;
                string hodnoty = $"{cislo}, {zamestnanec.Id}, {pokoj.Id}";

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
