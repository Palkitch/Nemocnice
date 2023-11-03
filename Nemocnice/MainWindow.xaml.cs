using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Controls;
using System.Windows.Data;

namespace Nemocnice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //OracleConnection connection;
        private List<String> Tables { get; set; }
        private DatabaseConnection DatabaseConnection { get; }
        private OracleConnection Connection { get; }
        private List<String> MezilehleTabulky { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Tables = new List<String>();
            DatabaseConnection = new DatabaseConnection();
            Connection = DatabaseConnection.OracleConnection;
            Connection.Open();
            MezilehleTabulky = new List<String>();
            MezilehleTabulkyInit();
            ComboBoxHandle();
        }


        public void ComboBoxHandle()
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

        private void printButtonOnAction(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Object> collection = new ObservableCollection<Object>();
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
                                    int id = int.Parse(read(reader, 0));
                                    int postNum = int.Parse(read(reader, 1));
                                    string street = read(reader, 2);
                                    string city = read(reader, 3);
                                    int postCode = int.Parse(read(reader, 4));
                                    string country = read(reader, 5);
                                    Adresa address = new Adresa(id, postNum, street, city, country, postCode);
                                    collection.Add(address);
                                }
                                break;


                            case "BUDOVY":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    int? floors = ParseNullableInt(read(reader, 2));
                                    int addressId = int.Parse(read(reader, 3));
                                    Budova building = new Budova(id, name, floors, addressId);
                                    collection.Add(building);
                                }
                                break;


                            case "DIAGNOZY_CISELNIK":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
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
                                                int id = int.Parse(read(cmdReader, 0));
                                                string name = read(cmdReader, 1);
                                                string surName = read(cmdReader, 2);
                                                int salary = int.Parse(read(cmdReader, 3));
                                                int hospWardId = int.Parse(read(cmdReader, 4));
                                                int? superiorId = ParseNullableInt(read(cmdReader, 5));
                                                int addressId = int.Parse(read(cmdReader, 6));
                                                char type = read(cmdReader, 7)[0];
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
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    string category = read(reader, 2);
                                    int price = int.Parse(read(reader, 3));
                                    Lek medicament = new Lek(id, name, category, price);  
                                    collection.Add(medicament); 
                                }
                                break;


                            case "LUZKA":
                                {
                                    // TODO: mby tohle nějak rozšiřit, že by se vypisovaly misto sestra_id_zamestanec informace o tom zaměstnanci
                                    int id = int.Parse(read(reader, 0));
                                    int bedNumber = int.Parse(read(reader, 1));
                                    int? nurseId = ParseNullableInt(read(reader, 2));
                                    int roomId = int.Parse(read(reader, 3));
                                    Luzko bed = new Luzko(id, bedNumber, nurseId, roomId);
                                    collection.Add(bed);
                                }
                                break;

                            case "ODDELENI":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    int buildingId = int.Parse(read(reader, 2));
                                    Oddeleni oddeleni = new Oddeleni(id, name, buildingId);
                                    collection.Add(oddeleni);
                                }
                                break;

                            case "PACIENTI":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    string surName = read(reader, 2);
                                    DateTime birthDate = DateTime.Parse(read(reader, 3));
                                    string formattedBirthDate = birthDate.ToString("yyyy-MM-dd");
                                    string pin = read(reader, 4);
                                    DateTime startDate = DateTime.Parse(read(reader, 5));
                                    string formattedStartDate = startDate.ToString("yyyy-MM-dd");
                                    int doctorId = int.Parse(read(reader, 6));
                                    int addressId = int.Parse(read(reader, 7));
                                    int insuranceId = int.Parse(read(reader, 8));
                                    Pacient pacient = new Pacient(id, name, surName, formattedBirthDate, pin, formattedStartDate, doctorId, addressId, insuranceId);
                                    collection.Add(pacient);    
                                }
                                break;

                            case "POJISTOVNY":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    int code = int.Parse(read(reader, 2));
                                    Pojistovna pojistovna = new Pojistovna(id, name, code);
                                    collection.Add(pojistovna);
                                }
                                break;

                            case "POKOJE":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    int roomNum = int.Parse(read(reader, 1));
                                    int hospWard = int.Parse(read(reader, 2));
                                    Pokoj pokoj = new Pokoj(id, roomNum, hospWard);
                                    collection.Add(pokoj);
                                }
                                break;

                            case "POMUCKY":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    int count = int.Parse(read(reader, 2));
                                    Pomucka pomucka = new Pomucka(id, name, count); 
                                    collection.Add(pomucka);
                                }
                                break;

                            case "RECEPTY":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    int docId = int.Parse(read(reader, 1));
                                    int patId = int.Parse(read(reader, 2));
                                    DateTime date = DateTime.Parse(read(reader, 3));
                                    string formattedDate = date.ToString("yyyy-MM-dd");
                                    Recept prescription = new Recept(id, docId, patId, formattedDate);
                                    collection.Add(prescription);
                                }
                                break;

                            case "SESTRY":
                                // Zpracování pro tabulku "SESTRY"

                                break;

                            case "ZAMESTNANCI":
                                {
                                    // Zpracování pro tabulku "ZAMESTNANCI"
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    string surname = read(reader, 2);
                                    int salary = int.Parse(read(reader, 3));
                                    int wardId = int.Parse(read(reader, 4));
                                    int? superiorId = ParseNullableInt(read(reader, 5));
                                    int addressId = int.Parse(read(reader, 6));
                                    char type = char.Parse(read(reader, 7));
                                    Zamestanec employee = new Zamestanec(id, name, surname, salary, wardId, superiorId, addressId, type);
                                    collection.Add(employee);
                                }
                                break;

                            default:
                                Console.WriteLine("Neznámá tabulka: ");
                                break;
                        }

                    }

                    try
                    {
                        grid.ItemsSource = collection;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        resultLabel.Content = "Chybný počet sloupců";
                    }
                }
            }
        }


        private string read(OracleDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? "..." : reader.GetString(columnIndex);
        }

        public int? ParseNullableInt(string input)
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


        private void MezilehleTabulkyInit()
        {
            MezilehleTabulky.AddRange(new List<String>
            {
                "LUZKA_PACIENTI", "ZAMESTNANCI_POMUCKY", "RECEPTY_LEKY", "DIAGNOZY_PACIENTI"
            });
        }
    }
}
