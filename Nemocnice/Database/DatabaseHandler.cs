using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Nemocnice.Database
{
    internal class DatabaseHandler
    {
        private Dictionary<string, string> TableAliasMapping { get; set; }
        private DatabaseConnection DatabaseConnection { get; }
        private OracleConnection Connection { get; }
        public Uzivatel Uzivatel { get; set; }

        public DatabaseHandler()
        {
            DatabaseConnection = DatabaseConnection.Instance;
            TableAliasMapping = new Dictionary<string, string>();
            Connection = DatabaseConnection.OracleConnection;
            Connection.Open();
        }

        public void ComboBoxHandle(ref ComboBox comboBox)
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


            using (var command = new OracleCommand("ZiskaniDat", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                string? selectedTableAlias = comboBox.SelectedItem.ToString();
                string? dictValue = TableAliasMapping.FirstOrDefault(x => x.Value == selectedTableAlias).Key;

                // Převedení názvu tabulky na velká písmena
                string? tableName = dictValue.ToUpper();

                command.CommandType = CommandType.StoredProcedure;

                // Přidání parametrů pro volání procedury
                command.Parameters.Add("p_table_name", OracleDbType.Varchar2).Value = dictValue;
                command.Parameters.Add("result_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                // Spuštění příkazu
                command.ExecuteNonQuery();
                OracleRefCursor refCursor = (OracleRefCursor)command.Parameters["result_cursor"].Value;


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


                            case "DOKTORI":
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
                                            reader.Close();
                                        }
                                    }
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

                            case "SESTRY":
                                // TODO: vyřešit sestry
                                break;

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
            grid.ItemsSource = collection;
        }

        public void saveImageToDatabase(string filePath)
        {
            try
            {
                byte[] imageBytes = LoadImageBytes(filePath);
                string? imageName = getImageNameFromFilePath(filePath);

                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = Connection;
                    cmd.CommandText = "INSERT INTO Ikony (nazev, obsah) VALUES (:imageName, :image)";
                    cmd.Parameters.Add("imageName", OracleDbType.Varchar2).Value = imageName;
                    cmd.Parameters.Add("image", OracleDbType.Blob).Value = imageBytes;

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Obrázek byl úspěšně uložen do databáze.", "Info");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při ukládání obrázku: " + ex.Message, "Chyba");
            }
        }

        public BitmapImage loadImageFromDatabase() 
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = Connection;
                    cmd.CommandText = "SELECT obsah FROM Ikony WHERE id_obrazek = 3";
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