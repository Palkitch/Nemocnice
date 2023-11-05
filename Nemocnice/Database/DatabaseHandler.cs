using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Nemocnice.Database
{
    internal class DatabaseHandler
    {
        private List<string> Tables { get; set; }
        private DatabaseConnection DatabaseConnection { get; }
        private OracleConnection Connection { get; }
        private List<string> MezilehleTabulky { get; set; }
        public DatabaseHandler()
        {
            DatabaseConnection = new DatabaseConnection();
            Tables = new List<string>();
            Connection = DatabaseConnection.OracleConnection;
            Connection.Open();
            MezilehleTabulky = new List<string>();
            MezilehleTabulkyInit();
        }
        public void ComboBoxHandle(ref ComboBox comboBox)
        {
            using (var command = new OracleCommand($"SELECT * FROM user_tables", Connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        Tables.Add(name);
                    }
                }
            }
            Tables.RemoveAll(name => MezilehleTabulky.Contains(name));
            foreach (string name in Tables)
            {
                comboBox.Items.Add(name);
            }
            comboBox.SelectedIndex = 0;
        }

        private void MezilehleTabulkyInit()
        {
            MezilehleTabulky.AddRange(new List<string>
            {
                "LUZKA_PACIENTI", "ZAMESTNANCI_POMUCKY", "RECEPTY_LEKY", "DIAGNOZY_PACIENTI"
            });
        }

        public void switchMethod(ref Label resultLabel, ref ComboBox comboBox, ref DataGrid grid)
        {
            {
                ObservableCollection<object> collection = new ObservableCollection<object>();
                resultLabel.Content = string.Empty;
                using (var command = new OracleCommand($"SELECT * FROM {comboBox.SelectedValue}", Connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (!reader.IsClosed && reader.Read())
                        {
                            switch (comboBox.SelectedValue)
                            {
                                case "ADRESY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        int postNum = int.Parse(ReadString(reader, 1));
                                        string street = ReadString(reader, 2);
                                        string city = ReadString(reader, 3);
                                        int postCode = int.Parse(ReadString(reader, 4));
                                        string country = ReadString(reader, 5);
                                        Adresa address = new Adresa(id, postNum, street, city, country, postCode);
                                        collection.Add(address);
                                    }
                                    break;


                                case "BUDOVY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string name = ReadString(reader, 1);
                                        int? floors = ParseNullableInt(ReadString(reader, 2));
                                        int addressId = int.Parse(ReadString(reader, 3));
                                        Budova building = new Budova(id, name, floors, addressId);
                                        collection.Add(building);
                                    }
                                    break;


                                case "DIAGNOZY_CISELNIK":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string name = ReadString(reader, 1);
                                        Diagnoza diagnosis = new Diagnoza(id, name);
                                        collection.Add(diagnosis);
                                    }
                                    break;

                                case "DOKTORI":
                                    {
                                        string sql = "SELECT z.* FROM ZAMESTNANCI z JOIN DOKTORI d ON z.id_zamestnanec = d.id_zamestnanec";
                                        using (var cmd = new OracleCommand(sql, Connection))
                                        {
                                            using (var cmdReader = cmd.ExecuteReader())
                                            {
                                                while (cmdReader.Read())
                                                {
                                                    int id = int.Parse(ReadString(cmdReader, 0));
                                                    string name = ReadString(cmdReader, 1);
                                                    string surName = ReadString(cmdReader, 2);
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
                                    }
                                    break;



                                case "LEKY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string name = ReadString(reader, 1);
                                        string category = ReadString(reader, 2);
                                        int price = int.Parse(ReadString(reader, 3));
                                        Lek medicament = new Lek(id, name, category, price);
                                        collection.Add(medicament);
                                    }
                                    break;


                                case "LUZKA":
                                    {
                                        // TODO: mby tohle nějak rozšiřit, že by se vypisovaly misto sestra_id_zamestanec informace o tom zaměstnanci
                                        int id = int.Parse(ReadString(reader, 0));
                                        int bedNumber = int.Parse(ReadString(reader, 1));
                                        int? nurseId = ParseNullableInt(ReadString(reader, 2));
                                        int roomId = int.Parse(ReadString(reader, 3));
                                        Luzko bed = new Luzko(id, bedNumber, nurseId, roomId);
                                        collection.Add(bed);
                                    }
                                    break;

                                case "ODDELENI":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string name = ReadString(reader, 1);
                                        int buildingId = int.Parse(ReadString(reader, 2));
                                        Oddeleni oddeleni = new Oddeleni(id, name, buildingId);
                                        collection.Add(oddeleni);
                                    }
                                    break;

                                case "PACIENTI":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string name = ReadString(reader, 1);
                                        string surName = ReadString(reader, 2);
                                        DateTime birthDate = DateTime.Parse(ReadString(reader, 3));
                                        string formattedBirthDate = birthDate.ToString("yyyy-MM-dd");
                                        string pin = ReadString(reader, 4);
                                        DateTime startDate = DateTime.Parse(ReadString(reader, 5));
                                        string formattedStartDate = startDate.ToString("yyyy-MM-dd");
                                        int doctorId = int.Parse(ReadString(reader, 6));
                                        int addressId = int.Parse(ReadString(reader, 7));
                                        int insuranceId = int.Parse(ReadString(reader, 8));
                                        Pacient pacient = new Pacient(id, name, surName, formattedBirthDate, pin, formattedStartDate, doctorId, addressId, insuranceId);
                                        collection.Add(pacient);
                                    }
                                    break;

                                case "POJISTOVNY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string name = ReadString(reader, 1);
                                        int code = int.Parse(ReadString(reader, 2));
                                        Pojistovna pojistovna = new Pojistovna(id, name, code);
                                        collection.Add(pojistovna);
                                    }
                                    break;

                                case "POKOJE":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        int roomNum = int.Parse(ReadString(reader, 1));
                                        int hospWard = int.Parse(ReadString(reader, 2));
                                        Pokoj pokoj = new Pokoj(id, roomNum, hospWard);
                                        collection.Add(pokoj);
                                    }
                                    break;

                                case "POMUCKY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string name = ReadString(reader, 1);
                                        int count = int.Parse(ReadString(reader, 2));
                                        Pomucka pomucka = new Pomucka(id, name, count);
                                        collection.Add(pomucka);
                                    }
                                    break;

                                case "RECEPTY":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        int docId = int.Parse(ReadString(reader, 1));
                                        int patId = int.Parse(ReadString(reader, 2));
                                        DateTime date = DateTime.Parse(ReadString(reader, 3));
                                        string formattedDate = date.ToString("yyyy-MM-dd");
                                        Recept prescription = new Recept(id, docId, patId, formattedDate);
                                        collection.Add(prescription);
                                    }
                                    break;

                                case "SESTRY":
                                    // TODO: vyřešit sestry
                                    break;

                                case "ZAMESTNANCI":
                                    {
                                        int id = int.Parse(ReadString(reader, 0));
                                        string name = ReadString(reader, 1);
                                        string surname = ReadString(reader, 2);
                                        int salary = int.Parse(ReadString(reader, 3));
                                        int wardId = int.Parse(ReadString(reader, 4));
                                        int? superiorId = ParseNullableInt(ReadString(reader, 5));
                                        int addressId = int.Parse(ReadString(reader, 6));
                                        char type = char.Parse(ReadString(reader, 7));
                                        Zamestanec employee = new Zamestanec(id, name, surname, salary, wardId, superiorId, addressId, type);
                                        collection.Add(employee);
                                    }
                                    break;

                                default:
                                    Console.WriteLine("Neznámá tabulka: ");
                                    break;
                            }

                        }
                        grid.ItemsSource = collection;
                    }
                }

            }
        }

        private static string ReadString(OracleDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? "..." : reader.GetString(columnIndex);
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