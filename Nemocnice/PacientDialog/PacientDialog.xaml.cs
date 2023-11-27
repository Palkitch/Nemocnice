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

namespace Nemocnice
{
    /// <summary>
    /// Interakční logika pro PacientDialog.xaml
    /// </summary>
    public partial class PacientDialog : Window
    {
        private OracleConnection connection;
        private List<Adresa> adresy;

        public PacientDialog(OracleConnection connection)
        {
            this.connection = connection;
            InitializeComponent();
            ComboBoxFill();
        }

        public PacientDialog(Pacient p, OracleConnection connection)
        {
            this.connection = connection;
            InitializeComponent();
        }

        #region ComboBox
        private void ComboBoxFill()
        {
            FillAdresaComboBox();
            FillDoktorComboBox();
            FillPojistovnaComboBox();
            FillKrevniSkupinaComboBox();
            FillDiagnozyComboBox();
        }
        private void FillDoktorComboBox()
        {
            try
            {
                doktorComboBox.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisDoktori", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("ID_ZAMESTNANEC"));
                            string jmeno = reader.GetString(reader.GetOrdinal("JMENO"));
                            string prijmeni = reader.GetString(reader.GetOrdinal("PRIJMENI"));
                            int plat = reader.GetInt32(reader.GetOrdinal("PLAT"));
                            int idOddeleni = reader.GetInt32(reader.GetOrdinal("ID_ODDELENI"));
                            int? idNadrizeneho = reader.IsDBNull(reader.GetOrdinal("ID_NADRIZENY")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ID_NADRIZENY"));
                            int idAdresy = reader.GetInt32(reader.GetOrdinal("ID_ADRESA"));
                            char druh = 'd';

                            // Vytvoření instance Zamestnanec z načtených dat
                            Zamestnanec doktor = new Zamestnanec(id, jmeno, prijmeni, plat, idOddeleni, idNadrizeneho, idAdresy, druh);

                            // Přidání doktora do ComboBoxu
                            doktorComboBox.Items.Add(doktor);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování doktorů: " + ex.Message);
            }
            doktorComboBox.SelectedIndex = 0;
        }
        private void FillAdresaComboBox()
        {
            try
            {
                adresaComboBox.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisAdresy", connection))
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
                                Convert.ToString(reader["ULICE"]),
                                Convert.ToString(reader["MESTO"]),
                                Convert.ToString(reader["STAT"]),
                                Convert.ToInt32(reader["PSC"])
                            );
                            adresaComboBox.Items.Add(adresa);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování adresy: " + ex.Message);
            }
            adresaComboBox.SelectedIndex = 0;
        }
        private void FillPojistovnaComboBox()
        {
            try
            {
                pojistovnaComboBox.Items.Clear();

                using (OracleCommand command = new OracleCommand("VypisPojistovny", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("ID_POJISTOVNA"));
                            string nazev = reader.GetString(reader.GetOrdinal("NAZEV"));
                            int cislo = reader.GetInt32(reader.GetOrdinal("CISLO"));

                            // Vytvoření instance Pojistovna z načtených dat
                            Pojistovna pojistovna = new Pojistovna(id, nazev, cislo);

                            // Přidání pojistovny do ComboBoxu
                            pojistovnaComboBox.Items.Add(pojistovna);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování pojistoven: " + ex.Message);
            }
            pojistovnaComboBox.SelectedIndex = 0;
        }
        private void FillKrevniSkupinaComboBox()
        {
            try
            {
                krevniSkupinaComboBox.Items.Clear();

                using (OracleCommand command = new OracleCommand("GetKrevniSkupiny", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("ID_SKUPINA"));
                            string typ = reader.GetString(reader.GetOrdinal("TYP"));

                            // Vytvoření instance KrevniSkupina z načtených dat
                            KrevniSkupina krevniSkupina = new KrevniSkupina(id, typ);

                            // Přidání krevní skupiny do ComboBoxu
                            krevniSkupinaComboBox.Items.Add(krevniSkupina);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování krevních skupin: " + ex.Message);
            }
            krevniSkupinaComboBox.SelectedIndex = 0;
        }
        private void FillDiagnozyComboBox()
        {
            try
            {
                diagnozaComboBox.Items.Clear();

                using (OracleCommand command = new OracleCommand("GetDiagnozy", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("ID"));
                            string nazev = reader.GetString(reader.GetOrdinal("NAZEV"));

                            Diagnoza diagnoza = new Diagnoza(id, nazev);

                            diagnozaComboBox.Items.Add(diagnoza);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při vyplňování diagnóz: " + ex.Message);
            }
            diagnozaComboBox.SelectedIndex = 0;
        }
        #endregion

        #region ButtonClick
        private void AddAdresaButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void AddPojistovnaButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void AddDiagnozaButtonClick(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
