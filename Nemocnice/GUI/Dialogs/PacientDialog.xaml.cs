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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Nemocnice
{
    /// <summary>
    /// Interakční logika pro PacientDialog.xaml
    /// </summary>
    public enum PacientDialogType
    {
        ADD, EDIT
    }
    public partial class PacientDialog
    {
        private OracleConnection connection;
        private List<Adresa> adresy;
        private Uzivatel uzivatel;
        private PacientDialogType type;
        private int pacientId;

        public PacientDialog(OracleConnection connection, Uzivatel u)
        {
            this.connection = connection;
            this.uzivatel = u;
            this.type = PacientDialogType.ADD;
            InitializeComponent();
            ComboBoxFill();
        }

        public PacientDialog(int p, OracleConnection connection, Uzivatel u)
        {
            this.connection = connection;
            this.uzivatel = u;
            this.pacientId = p;
            this.type = PacientDialogType.EDIT;
            InitializeComponent();
            ComboBoxFill();
            LoadPatientData();
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
        private void HotovoOnClick(object sender, RoutedEventArgs e)
        {
            switch (type)
            {
                case PacientDialogType.ADD:
                    {
                        AddPacient();
                        break;
                    }
                case PacientDialogType.EDIT:
                    {
                        EditPacient();
                        break;
                    }
            }
            this.Close();
        }
        #endregion

        #region Funkcionalita
        private void AddPacient()
        {
            switch (uzivatel.Role)
            {
                case Role.PRIMAR:
                    {
                        {
                            try
                            {
                                using (OracleCommand command = new OracleCommand("insert_data", connection))
                                {
                                    command.CommandType = CommandType.StoredProcedure;

                                    // Parametry procedury
                                    command.Parameters.Add("p_tabulka", OracleDbType.Varchar2).Value = "Pacienti";
                                    command.Parameters.Add("p_sloupce", OracleDbType.Varchar2).Value = "JMENO, PRIJMENI, DATUM_NAROZENI, RODNE_CISLO, DATUM_NASTUPU, ID_DOKTOR, ID_ADRESA, ID_POJISTOVNA, ID_SKUPINA, ID_DIAGNOZA";

                                    // Získání hodnot z UI
                                    string jmeno = jmenoTextBox.Text;
                                    string prijmeni = prijmeniTextBox.Text;
                                    DateTime datumNarozeni = datumNarozeniDatePicker.SelectedDate ?? DateTime.Now; // Default na dnešní datum
                                    string rodneCislo = rodneCisloTextBox.Text;
                                    DateTime datumNastupu = datumNastupuDatePicker.SelectedDate ?? DateTime.Now; // Default na dnešní datum
                                    int idDoktor = ((Zamestnanec)doktorComboBox.SelectedItem).Id;
                                    int idAdresa = ((Adresa)adresaComboBox.SelectedItem).Id;
                                    int idPojistovna = ((Pojistovna)pojistovnaComboBox.SelectedItem).Id;
                                    int idSkupina = ((KrevniSkupina)krevniSkupinaComboBox.SelectedItem).Id;
                                    int idDiagnoza = ((Diagnoza)diagnozaComboBox.SelectedItem).Id;

                                    // Parametry pro hodnoty
                                    string hodnoty = $"'{jmeno}', '{prijmeni}', TO_DATE('{datumNarozeni.ToString("yyyy-MM-dd")}', 'YYYY-MM-DD'), '{rodneCislo}', TO_DATE('{datumNastupu.ToString("yyyy-MM-dd")}', 'YYYY-MM-DD'), {idDoktor}, {idAdresa}, {idPojistovna}, {idSkupina}, {idDiagnoza}";

                                    command.Parameters.Add("p_hodnoty", OracleDbType.Varchar2).Value = hodnoty;

                                    // Spuštění procedury
                                    command.ExecuteNonQuery();

                                    MessageBox.Show("Pacient úspěšně přidán.");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Chyba při přidávání pacienta: " + ex.Message);
                            }
                        }

                        break;
                    }
                case Role.DOKTOR:
                case Role.SESTRA:
                    {
                        using (OracleCommand command = new OracleCommand("VytvorZadostPacienti", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            // Získání hodnot z UI
                            string jmeno = jmenoTextBox.Text;
                            string prijmeni = prijmeniTextBox.Text;
                            DateTime datumNarozeni = datumNarozeniDatePicker.SelectedDate ?? DateTime.Now;
                            string rodneCislo = rodneCisloTextBox.Text;
                            DateTime datumNastupu = datumNastupuDatePicker.SelectedDate ?? DateTime.Now;
                            int idDoktor = ((Zamestnanec)doktorComboBox.SelectedItem).Id;
                            int idAdresa = ((Adresa)adresaComboBox.SelectedItem).Id;
                            int idPojistovna = ((Pojistovna)pojistovnaComboBox.SelectedItem).Id;
                            int idSkupina = ((KrevniSkupina)krevniSkupinaComboBox.SelectedItem).Id;
                            int idDiagnoza = ((Diagnoza)diagnozaComboBox.SelectedItem).Id;

                            // Parametry pro volání procedury VytvorZadostPacienti
                            command.Parameters.Add("p_Jmeno", OracleDbType.Varchar2).Value = jmeno;
                            command.Parameters.Add("p_Prijmeni", OracleDbType.Varchar2).Value = prijmeni;
                            command.Parameters.Add("p_DatumNarozeni", OracleDbType.Date).Value = datumNarozeni;
                            command.Parameters.Add("p_RodneCislo", OracleDbType.Varchar2).Value = rodneCislo;
                            command.Parameters.Add("p_DatumNastupu", OracleDbType.Date).Value = datumNastupu;
                            command.Parameters.Add("p_IdDoktor", OracleDbType.Int32).Value = idDoktor;
                            command.Parameters.Add("p_IdAdresa", OracleDbType.Int32).Value = idAdresa;
                            command.Parameters.Add("p_IdPojistovna", OracleDbType.Int32).Value = idPojistovna;
                            command.Parameters.Add("p_IdSkupina", OracleDbType.Int32).Value = idSkupina;
                            command.Parameters.Add("p_IdDiagnoza", OracleDbType.Int32).Value = idDiagnoza;

                            command.Parameters.Add("p_DruhZadosti", OracleDbType.Char).Value = 'A';
                            // Spuštění procedury
                            command.ExecuteNonQuery();

                            MessageBox.Show("Pacient úspěšně přidán ke schválení.");
                        }
                        break;
                    }
            }
        }
        private void EditPacient()
        {
            switch (uzivatel.Role)
            {
                case Role.PRIMAR:
                    {
                        int id = pacientId;
                        string jmeno = jmenoTextBox.Text;
                        string prijmeni = prijmeniTextBox.Text;
                        DateTime datumNarozeni = datumNarozeniDatePicker.SelectedDate ?? DateTime.Now;
                        string rodneCislo = rodneCisloTextBox.Text;
                        DateTime datumNastupu = datumNastupuDatePicker.SelectedDate ?? DateTime.Now;
                        int idDoktor = ((Zamestnanec)doktorComboBox.SelectedItem).Id;
                        int idAdresa = ((Adresa)adresaComboBox.SelectedItem).Id;
                        int idPojistovna = ((Pojistovna)pojistovnaComboBox.SelectedItem).Id;
                        int idSkupina = ((KrevniSkupina)krevniSkupinaComboBox.SelectedItem).Id;
                        int idDiagnoza = ((Diagnoza)diagnozaComboBox.SelectedItem).Id;

                        UpdatePacientData(id, jmeno, prijmeni, datumNarozeni, rodneCislo, datumNastupu, idDoktor, idAdresa, idPojistovna, idSkupina, idDiagnoza);
                        break;
                    }
                case Role.DOKTOR:
                case Role.SESTRA:
                    {
                        using (OracleCommand command = new OracleCommand("VytvorZadostPacienti", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            // Získání hodnot z UI
                            string jmeno = jmenoTextBox.Text;
                            string prijmeni = prijmeniTextBox.Text;
                            DateTime datumNarozeni = datumNarozeniDatePicker.SelectedDate ?? DateTime.Now;
                            string rodneCislo = rodneCisloTextBox.Text;
                            DateTime datumNastupu = datumNastupuDatePicker.SelectedDate ?? DateTime.Now;
                            int idDoktor = ((Zamestnanec)doktorComboBox.SelectedItem).Id;
                            int idAdresa = ((Adresa)adresaComboBox.SelectedItem).Id;
                            int idPojistovna = ((Pojistovna)pojistovnaComboBox.SelectedItem).Id;
                            int idSkupina = ((KrevniSkupina)krevniSkupinaComboBox.SelectedItem).Id;
                            int idDiagnoza = ((Diagnoza)diagnozaComboBox.SelectedItem).Id;

                            // Parametry pro volání procedury VytvorZadostPacienti
                            command.Parameters.Add("p_IdPacient", OracleDbType.Int32).Value = pacientId;
                            command.Parameters.Add("p_Jmeno", OracleDbType.Varchar2).Value = jmeno;
                            command.Parameters.Add("p_Prijmeni", OracleDbType.Varchar2).Value = prijmeni;
                            command.Parameters.Add("p_DatumNarozeni", OracleDbType.Date).Value = datumNarozeni;
                            command.Parameters.Add("p_RodneCislo", OracleDbType.Varchar2).Value = rodneCislo;
                            command.Parameters.Add("p_DatumNastupu", OracleDbType.Date).Value = datumNastupu;
                            command.Parameters.Add("p_IdDoktor", OracleDbType.Int32).Value = idDoktor;
                            command.Parameters.Add("p_IdAdresa", OracleDbType.Int32).Value = idAdresa;
                            command.Parameters.Add("p_IdPojistovna", OracleDbType.Int32).Value = idPojistovna;
                            command.Parameters.Add("p_IdSkupina", OracleDbType.Int32).Value = idSkupina;
                            command.Parameters.Add("p_IdDiagnoza", OracleDbType.Int32).Value = idDiagnoza;

                            // Přidání parametru pro typ záznamu ('E' pro editaci)
                            command.Parameters.Add("p_DruhZadosti", OracleDbType.Char).Value = 'E'; // 'E' znamená úpravu (EDIT)

                            // Spuštění procedury
                            command.ExecuteNonQuery();

                            MessageBox.Show("Pacient úspěšně upraven ke schválení.");
                        }
                        break;
                    }
            }
        }
        private void UpdatePacientData(int idPacienta, string jmeno, string prijmeni, DateTime datumNarozeni, string rodneCislo, DateTime datumNastupu, int idDoktor, int idAdresa, int idPojistovna, int idSkupina, int idDiagnoza)
        {
            try
            {
                using (OracleCommand command = new OracleCommand("update_data", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_tabulka", OracleDbType.Varchar2).Value = "Pacienti";
                    command.Parameters.Add("p_set_clause", OracleDbType.Varchar2).Value = $"JMENO = '{jmeno}', PRIJMENI = '{prijmeni}', DATUM_NAROZENI = TO_DATE('{datumNarozeni.ToString("yyyy-MM-dd")}', 'YYYY-MM-DD'), RODNE_CISLO = '{rodneCislo}', DATUM_NASTUPU = TO_DATE('{datumNastupu.ToString("yyyy-MM-dd")}', 'YYYY-MM-DD'), ID_DOKTOR = {idDoktor}, ID_ADRESA = {idAdresa}, ID_POJISTOVNA = {idPojistovna}, ID_SKUPINA = {idSkupina}, ID_DIAGNOZA = {idDiagnoza}";
                    command.Parameters.Add("p_where_clause", OracleDbType.Varchar2).Value = $"ID_PACIENT = {idPacienta}";

                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Data pacienta byla úspěšně aktualizována.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při aktualizaci dat: {ex.Message}");
            }
        }
        private void LoadPatientData()
        {
            try
            {
                using (OracleCommand command = new OracleCommand("NactiHodnotyPacienta", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_IdPacient", OracleDbType.Int32).Value = pacientId;
                    command.Parameters.Add("cur", OracleDbType.RefCursor, ParameterDirection.Output);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            jmenoTextBox.Text = reader["JMENO"].ToString();
                            prijmeniTextBox.Text = reader["PRIJMENI"].ToString();
                            datumNarozeniDatePicker.SelectedDate = (DateTime)reader["DATUM_NAROZENI"];
                            rodneCisloTextBox.Text = reader["RODNE_CISLO"].ToString();
                            datumNastupuDatePicker.SelectedDate = (DateTime)reader["DATUM_NASTUPU"];

                            int idDoktor = Convert.ToInt32(reader["ID_DOKTOR"]);
                            Zamestnanec vybranyDoktor = doktorComboBox.Items.OfType<Zamestnanec>().FirstOrDefault(d => d.Id == idDoktor);
                            doktorComboBox.SelectedItem = vybranyDoktor;

                            int idAdresa = Convert.ToInt32(reader["ID_ADRESA"]);
                            Adresa vybranaAdresa = adresaComboBox.Items.OfType<Adresa>().FirstOrDefault(d => d.Id == idAdresa);
                            adresaComboBox.SelectedItem = vybranaAdresa;

                            int idPojistovna = Convert.ToInt32(reader["ID_POJISTOVNA"]);
                            Pojistovna vybranaPojistovna = pojistovnaComboBox.Items.OfType<Pojistovna>().FirstOrDefault(d => d.Id == idPojistovna);
                            pojistovnaComboBox.SelectedItem = vybranaPojistovna;

                            int idSkupina = Convert.ToInt32(reader["ID_SKUPINA"]);
                            KrevniSkupina vybranaKrevniSkupina = krevniSkupinaComboBox.Items.OfType<KrevniSkupina>().FirstOrDefault(d => d.Id == idSkupina);
                            krevniSkupinaComboBox.SelectedItem = vybranaKrevniSkupina;

                            int idDiagnoza = Convert.ToInt32(reader["ID_DIAGNOZA"]);
                            Diagnoza vybranaDiagnoza = diagnozaComboBox.Items.OfType<Diagnoza>().FirstOrDefault(d => d.Id == idDiagnoza);
                            diagnozaComboBox.SelectedItem = vybranaDiagnoza;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání dat pacienta: " + ex.Message);
            }
        }
        #endregion
    }
}
