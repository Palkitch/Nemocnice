using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Nemocnice.Database
{
    public class DatabaseHandler
    {
        private Dictionary<string, string> TableAliasMapping { get; set; }
        private DatabaseConnection DatabaseConnection { get; }
        private OracleConnection Connection { get; }
        //public Uzivatel? Uzivatel { get; set; }
        private static DatabaseHandler? instance;

        public DatabaseHandler()
        {
            DatabaseConnection = DatabaseConnection.Instance;
            TableAliasMapping = new Dictionary<string, string>();
            Connection = DatabaseConnection.OracleConnection;
            Connection.Open();
        }

        public static DatabaseHandler Instance  // singletone kvuli otevirani connection v konstruktoru
        {
            get
            {
                instance ??= new DatabaseHandler();
                return instance;
            }
        }

        public void adminComboBoxHandle(ref ComboBox comboBox)   // TODO: asi rename, takhle se to stejně řešit nebude, leda pro admina
        {
            {
                using (OracleCommand command = new OracleCommand("ZobrazeniTabulek", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("result_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                    command.ExecuteNonQuery();

                    OracleDataReader reader = ((OracleRefCursor)command.Parameters["result_cursor"].Value).GetDataReader();
                    StringBuilder sb = new StringBuilder();
                    while (reader.Read())
                    {
                        string? aliasTableName = reader["alias_table_name"].ToString();
                        string? tableName = reader["table_name"].ToString();
                        sb.AppendLine(tableName + " (" + aliasTableName + ")");
                        TableAliasMapping.Add(tableName, aliasTableName);
                    }
                }
                foreach (string value in TableAliasMapping.Values)
                {
                    comboBox.Items.Add(value);
                }
                comboBox.SelectedIndex = 0;
            }
        }


        public void SwitchMethod(ref ComboBox comboBox, ref DataGrid grid)
        {

            ObservableCollection<object> collection = new ObservableCollection<object>();

            string? selectedTableAlias = comboBox.SelectedItem.ToString();
            string? dictValue = TableAliasMapping.FirstOrDefault(x => x.Value == selectedTableAlias).Key;


            using (var command = new OracleCommand("ZiskaniDat", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Převedení názvu tabulky na velká písmena
                string? tableName = dictValue.ToUpper();

                command.CommandType = CommandType.StoredProcedure;

                // Přidání parametrů pro volání procedury
                command.Parameters.Add("p_table_name", OracleDbType.Varchar2).Value = dictValue;
                command.Parameters.Add("result_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                // Spuštění příkazu
                command.ExecuteNonQuery();
                OracleRefCursor refCursor = (OracleRefCursor)command.Parameters["result_cursor"].Value;

                if (dictValue == "DOKTORI")
                {
                    string? sql = "SELECT z.* FROM ZAMESTNANCI z JOIN DOKTORI d ON z.id_zamestnanec = d.id_zamestnanec";
                    using (var cmd = new OracleCommand(sql, Connection))
                    {
                        using (var cmdReader = cmd.ExecuteReader())
                        {
                            while (cmdReader.Read())
                            {
                                int id = int.Parse(ReadString(cmdReader, 0));
                                string? name = ReadString(cmdReader, 1);
                                string? surName = ReadString(cmdReader, 2);
                                int salary = int.Parse(ReadString(cmdReader, 3));
                                int hospWardId = int.Parse(ReadString(cmdReader, 4));
                                int? superiorId = ParseNullableInt(ReadString(cmdReader, 5));
                                int addressId = int.Parse(ReadString(cmdReader, 6));
                                char type = ReadString(cmdReader, 7)[0];
                                Zamestanec employee = new Zamestanec(id, name, surName, salary, hospWardId, superiorId, addressId, type);
                                collection.Add(employee);
                            }
                            cmdReader.Close();
                        }
                    }
                }
                else if (dictValue == "SESTRY")
                {
                    string? sql = "SELECT z.* FROM ZAMESTNANCI z JOIN SESTRY s ON z.id_zamestnanec = s.id_zamestnanec";
                    using (var cmd = new OracleCommand(sql, Connection))
                    {
                        using (var cmdReader = cmd.ExecuteReader())
                        {
                            while (cmdReader.Read())
                            {
                                int id = int.Parse(ReadString(cmdReader, 0));
                                string? name = ReadString(cmdReader, 1);
                                string? surName = ReadString(cmdReader, 2);
                                int salary = int.Parse(ReadString(cmdReader, 3));
                                int hospWardId = int.Parse(ReadString(cmdReader, 4));
                                int? superiorId = ParseNullableInt(ReadString(cmdReader, 5));
                                int addressId = int.Parse(ReadString(cmdReader, 6));
                                char type = ReadString(cmdReader, 7)[0];
                                Zamestanec employee = new Zamestanec(id, name, surName, salary, hospWardId, superiorId, addressId, type);
                                collection.Add(employee);
                            }
                            cmdReader.Close();
                        }
                    }
                }
                else
                {
                    using (OracleDataReader reader = refCursor.GetDataReader())
                    {
                        while (reader.Read())
                        {

                            switch (dictValue)
                            {
                                case "ADRESY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        int postNum = int.Parse(ReadString(reader, 1));
                                        string? street = ReadString(reader, 2);
                                        string? city = ReadString(reader, 3);
                                        int postCode = int.Parse(ReadString(reader, 4));
                                        string? country = ReadString(reader, 5);
                                        Adresa address = new Adresa(id, postNum, street, city, country, postCode);
                                        collection.Add(address);
                                    }
                                    break;


                                case "BUDOVY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string? name = ReadString(reader, 1);
                                        int? floors = ParseNullableInt(ReadString(reader, 2));
                                        int addressId = int.Parse(ReadString(reader, 3));
                                        Budova building = new Budova(id, name, floors, addressId);
                                        collection.Add(building);
                                        break;
                                    }



                                case "DIAGNOZY_CISELNIK":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string? name = ReadString(reader, 1);
                                        Diagnoza diagnosis = new Diagnoza(id, name);
                                        collection.Add(diagnosis);
                                        break;
                                    }
                                case "LEKY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string? name = ReadString(reader, 1);
                                        string? category = ReadString(reader, 2);
                                        int price = int.Parse(ReadString(reader, 3));
                                        Lek medicament = new Lek(id, name, category, price);
                                        collection.Add(medicament);
                                        break;
                                    }

                                case "LUZKA":
                                    {
                                        // TODO: mby tohle nějak rozšiřit, že by se vypisovaly misto sestra_id_zamestanec informace o tom zaměstnanci
                                        // pro admina bych nechal tyhle data, pro usera bych to nějakým způsobem rozšířil
                                        int id = int.Parse(ReadString(reader, 0));
                                        int bedNumber = int.Parse(ReadString(reader, 1));
                                        int? nurseId = ParseNullableInt(ReadString(reader, 2));
                                        int roomId = int.Parse(ReadString(reader, 3));
                                        Luzko bed = new Luzko(id, bedNumber, nurseId, roomId);
                                        collection.Add(bed);
                                        break;
                                    }

                                case "ODDELENI":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string? name = ReadString(reader, 1);
                                        int buildingId = int.Parse(ReadString(reader, 2));
                                        Oddeleni oddeleni = new Oddeleni(id, name, buildingId);
                                        collection.Add(oddeleni);
                                        break;
                                    }

                                case "PACIENTI":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string? name = ReadString(reader, 1);
                                        string? surName = ReadString(reader, 2);
                                        DateTime birthDate = DateTime.Parse(ReadString(reader, 3));
                                        string? formattedBirthDate = birthDate.ToString("yyyy-MM-dd");
                                        string? pin = ReadString(reader, 4);
                                        DateTime startDate = DateTime.Parse(ReadString(reader, 5));
                                        string? formattedStartDate = startDate.ToString("yyyy-MM-dd");
                                        int doctorId = int.Parse(ReadString(reader, 6));
                                        int addressId = int.Parse(ReadString(reader, 7));
                                        int insuranceId = int.Parse(ReadString(reader, 8));
                                        Pacient pacient = new Pacient(id, name, surName, formattedBirthDate, pin, formattedStartDate, doctorId, addressId, insuranceId);
                                        collection.Add(pacient);
                                        break;
                                    }

                                case "POJISTOVNY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string? name = ReadString(reader, 1);
                                        int code = int.Parse(ReadString(reader, 2));
                                        Pojistovna pojistovna = new Pojistovna(id, name, code);
                                        collection.Add(pojistovna);
                                        break;
                                    }

                                case "POKOJE":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        int roomNum = int.Parse(ReadString(reader, 1));
                                        int hospWard = int.Parse(ReadString(reader, 2));
                                        Pokoj pokoj = new Pokoj(id, roomNum, hospWard);
                                        collection.Add(pokoj);
                                        break;
                                    }

                                case "POMUCKY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string? name = ReadString(reader, 1);
                                        int count = int.Parse(ReadString(reader, 2));
                                        Pomucka pomucka = new Pomucka(id, name, count);
                                        collection.Add(pomucka);
                                        break;
                                    }

                                case "RECEPTY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        int docId = int.Parse(ReadString(reader, 1));
                                        int patId = int.Parse(ReadString(reader, 2));
                                        DateTime date = DateTime.Parse(ReadString(reader, 3));
                                        string? formattedDate = date.ToString("yyyy-MM-dd");
                                        Recept prescription = new Recept(id, docId, patId, formattedDate);
                                        collection.Add(prescription);
                                        break;
                                    }
                                case "ZAMESTNANCI":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string? name = ReadString(reader, 1);
                                        string? surname = ReadString(reader, 2);
                                        int salary = int.Parse(ReadString(reader, 3));
                                        int wardId = int.Parse(ReadString(reader, 4));
                                        int? superiorId = ParseNullableInt(ReadString(reader, 5));
                                        int addressId = int.Parse(ReadString(reader, 6));
                                        char type = char.Parse(ReadString(reader, 7));
                                        Zamestanec employee = new Zamestanec(id, name, surname, salary, wardId, superiorId, addressId, type);
                                        collection.Add(employee);
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            grid.ItemsSource = collection;
            grid.Columns[0].Visibility = Visibility.Hidden; // TODO: Ne moc dobrý řešení toho, jestli je vidět id nebo není. Mohlo by se to lišit podle role. Jestli tě napadá lepší řešení tak řekni
        }

        public int saveImageToDatabase(string filePath)
        {
            try
            {
                byte[] imageBytes = LoadImageBytes(filePath);
                string? imageName = getImageNameFromFilePath(filePath);
                int newId;
                using (OracleCommand cmd = new OracleCommand())
                {
                    // Uložení obrazku do ikon
                    cmd.Connection = Connection;
                    cmd.CommandText = "INSERT INTO Ikony (nazev, obsah) VALUES (:imageName, :image) RETURNING id INTO :newId";
                    cmd.Parameters.Add("imageName", OracleDbType.Varchar2).Value = imageName;
                    cmd.Parameters.Add("image", OracleDbType.Blob).Value = imageBytes;
                    OracleParameter newIdParam = new OracleParameter(":newId", OracleDbType.Int32);
                    newIdParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(newIdParam);
                    cmd.ExecuteNonQuery();

                    // update obrázku aktuálně přihlášeného uživatele
                    newId = Convert.ToInt32(newIdParam.Value);
                    cmd.CommandText = "UPDATE uzivatele SET id_obrazku = :newId WHERE id_uzivatel = (SELECT * FROM prihlaseny_uzivatel)";
                    cmd.Parameters.Add("newId", OracleDbType.Int32).Value = newId;
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Obrázek byl úspěšně uložen do databáze.", "Info");
                return newId;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při ukládání obrázku: " + ex.Message, "Chyba");
            }
            return 0;
        }

        public string? Login(string username, string password)
        {
            if (!UserExists(username))
            {
                MessageBox.Show($"Uživatel s jménem {username} neexistuje.", "Upozornění");
                return null;
            }

            string storedHashedPassword = GetHashedPassword(username);
            return storedHashedPassword == password ? storedHashedPassword : null;
        }

        public bool Register(string username, string password)
        {
            if (UserExists(username))
            {
                MessageBox.Show($"Uživatel s jménem {username} již existuje.", "Upozornění");
                return false;
            }
            string insertQuery = "INSERT INTO Uzivatele (nazev, role, id_obrazku, heslo) VALUES (:uname, :urole, :image_id, :pwd)";
            OracleCommand cmd = new OracleCommand(insertQuery, Connection);
            cmd.Parameters.Add(new OracleParameter("uname", username));
            cmd.Parameters.Add(new OracleParameter("urole", "user"));
            cmd.Parameters.Add(new OracleParameter("image_id", 2));
            cmd.Parameters.Add(new OracleParameter("pwd", password));
            cmd.ExecuteNonQuery();

            return true;
        }

        private bool UserExists(string username)
        {
            string query = "SELECT COUNT(*) FROM Uzivatele WHERE nazev = :uname";
            OracleCommand cmd = new OracleCommand(query, Connection);
            cmd.Parameters.Add(new OracleParameter("uname", username));
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        private string GetHashedPassword(string username)
        {
            string query = "SELECT heslo FROM Uzivatele WHERE nazev = :uname";
            OracleCommand cmd = new OracleCommand(query, Connection);
            cmd.Parameters.Add(new OracleParameter("uname", username));
            return cmd.ExecuteScalar().ToString();
        }

        public BitmapImage loadImageFromDatabase(int idImage)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = Connection;
                    cmd.CommandText = "SELECT obsah FROM Ikony WHERE id_obrazek = :idImage";
                    cmd.Parameters.Add(new OracleParameter("idImage", idImage));
                    OracleDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Načítání bytů z databáze
                        byte[] imageBytes = (byte[])reader["obsah"];

                        // Vytvoření BitmapImage z načtených bytů
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = new MemoryStream(imageBytes);
                        bitmap.EndInit();
                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání obrázku: " + ex.Message, "Chyba");
            }
            return null;
        }

        private string? getImageNameFromFilePath(string filePath)
        {
            string[] parts = filePath.Split('\\', '/');
            string fileName = parts[parts.Length - 1];
            return fileName;
        }

        private byte[] LoadImageBytes(string filePath)
        {
            byte[] imageBytes = File.ReadAllBytes(filePath);
            return imageBytes;
        }

        public void loadLoggedUser(TextBox tb, ComboBox cb, Image img)
        {
            Uzivatel uzivatel = null;
            BitmapImage bitMapImage = null;

            string query = "SELECT prihlaseny_nazev, obrazek_id, prihlaseny_role FROM uzivatele_s_obrazky";
            OracleCommand cmd = new OracleCommand(query, Connection);
            using (var cmdReader = cmd.ExecuteReader())
            {
                while (cmdReader.Read())
                {
                    string? name = ReadString(cmdReader, 0);
                    int imgId = int.Parse(ReadString(cmdReader, 1));
                    string? roleString = ReadString(cmdReader, 2).ToUpper();
                    if (Enum.TryParse<Role>(roleString, out Role role))
                    {
                        uzivatel = new Uzivatel(name, role);
                    }
                    bitMapImage = loadImageFromDatabase(imgId);
                }
            }
            if (uzivatel != null)
            {
                tb.Text = uzivatel.Jmeno;
                tb.IsReadOnly = true;
                cb.Items.Add(uzivatel.Role.ToString().ToLower());
                cb.IsReadOnly = true;
                cb.SelectedIndex = 0;
                img.Source = bitMapImage;
            }


        }

        private static string ReadString(OracleDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? "..." : reader.GetString(columnIndex);
        }

        public void NastaveniUzivatele(string Jmeno, Role role)
        {

        }

        private static int? ParseNullableInt(string input)
        {
            if (int.TryParse(input, out int result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}