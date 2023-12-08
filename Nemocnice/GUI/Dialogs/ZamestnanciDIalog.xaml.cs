using Nemocnice.Config;
using Nemocnice.Database;
using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Window = System.Windows.Window;

namespace Nemocnice.GUI.Dialogs
{
    /// <summary>
    /// Interakční logika pro ZamestnanciDialog.xaml
    /// </summary>
    public partial class ZamestnanciDialog : Window
    {
        public OracleConnection Connection { get; }
        string typZamestance;

        public ZamestnanciDialog(string typZamestance)
        {
            this.typZamestance = typZamestance;
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

            try // oddeleni cb fill
            {
                OddeleniCb.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisOddeleni", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Oddeleni oddeleni = new Oddeleni(
                                reader.GetInt32("id_oddeleni"),
                                reader.GetString("nazev"),
                                reader.GetInt32("id_budova"));
                            OddeleniCb.Items.Add(oddeleni);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování Kategorie: " + ex.Message);
            }
            OddeleniCb.SelectedIndex = 0;

            // druh cb fill
            DruhCb.Items.Add("S");
            DruhCb.Items.Add("D");
            if (typZamestance == "d")
                DruhCb.SelectedIndex = 1;
            else if (typZamestance == "s")
                DruhCb.SelectedIndex = 0;

            try // nadrizeny cb fill
            {
                NadrizenyCb.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisNadrizeny", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Zamestnanec nadrizeny = new Zamestnanec(
                                Convert.ToInt32(reader["id_zamestnanec"]),
                                reader.GetString("jmeno"),
                                reader.GetString("prijmeni")
                            );
                            NadrizenyCb.Items.Add(nadrizeny);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování Nadrizeneho: " + ex.Message);
            }
            NadrizenyCb.SelectedIndex = 0;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            int plat;   // 5ti mistny
            if (InputValidator.ParsableIntFromTextBox(PlatTb, "Plat", out plat))
            try
            {
                Oddeleni? oddeleni = OddeleniCb.SelectedItem as Oddeleni;
                Zamestnanec? zamestnanec = NadrizenyCb.SelectedItem as Zamestnanec;
                Adresa? adresa = AdresaCb.SelectedItem as Adresa;
                string? druh = DruhCb.SelectedItem as string;
                int id_noveho;
                using (OracleCommand cmd = new OracleCommand("vloz_zamestnance", Connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_jmeno", OracleDbType.Varchar2).Value = JmenoTb.Text;
                    cmd.Parameters.Add("p_prijmeni", OracleDbType.Varchar2).Value = PrijmeniTb.Text;
                    cmd.Parameters.Add("p_plat", OracleDbType.Int32).Value = plat;
                    cmd.Parameters.Add("p_id_oddeleni", OracleDbType.Int32).Value = oddeleni.ID;
                    cmd.Parameters.Add("p_id_nadrizeny", OracleDbType.Int32).Value = zamestnanec.Id;
                    cmd.Parameters.Add("p_id_adresa", OracleDbType.Int32).Value = adresa.Id;
                    cmd.Parameters.Add("p_druh", OracleDbType.Char).Value = druh;

                    cmd.ExecuteNonQuery();

                    using (OracleCommand select = new OracleCommand("SELECT MAX(id_zamestnanec) FROM zamestnanci", Connection))
                    {
                        id_noveho = Convert.ToInt32((decimal)select.ExecuteScalar());
                    }

                    if (druh == "S")
                    {
                        using (OracleCommand insertSestra = new OracleCommand("INSERT INTO sestry VALUES(:id_sestry)", Connection))
                        {
                            insertSestra.Parameters.Add(new OracleParameter("id_sestry", id_noveho));
                            insertSestra.ExecuteNonQuery();
                        }
                    }
                    else if (druh == "D") 
                    {
                        using (OracleCommand insertDoktor = new OracleCommand("INSERT INTO doktori VALUES(:id_doktora)", Connection))
                        {
                            insertDoktor.Parameters.Add(new OracleParameter("id_doktora", id_noveho));
                            insertDoktor.ExecuteNonQuery();
                        }
                    }
                    // Data byla úspěšně vložena
                    MessageBox.Show("Data byla úspěšně vložena do tabulky zamesnanci", "Úspěch", MessageBoxButton.OK, MessageBoxImage.Information);
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
