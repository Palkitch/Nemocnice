using Nemocnice.Config;
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
    /// Interakční logika pro LekyDialog.xaml
    /// </summary>
    public partial class LekyDialog : Window
    {
        public OracleConnection Connection { get; }

        public LekyDialog()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DatabaseConnection db = DatabaseConnection.Instance;
            Connection = db.OracleConnection;
            InitializeComponent();
            InitComboBoxes();
        }

        private void InitComboBoxes()
        {
            try // kategorie cb fill
            {
                KategorieCb.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisKategorie", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            KategorieLeku kategorie = new KategorieLeku(
                                Convert.ToInt32(reader["id_kategorie"]), reader.GetString("nazev_kategorie"));
                            KategorieCb.Items.Add(kategorie);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování Kategorie: " + ex.Message);
            }
            KategorieCb.SelectedIndex = 0;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            int cena;
            if (InputValidator.ParsableIntFromTextBox(CenaTb, "Cena", out cena))
            try
            {
                string tabulka = "leky";
                string sloupce = "nazev, cena, id_kategorie";
                KategorieLeku? kategorie = KategorieCb.SelectedItem as KategorieLeku;
                string hodnoty = $"'{NazevTb.Text}', {cena}, {kategorie.Id}";

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
