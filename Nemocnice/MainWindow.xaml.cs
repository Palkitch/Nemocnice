using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;

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

                    while (reader.Read())
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
                                    Address address = new Address(id, postNum, street, city, country, postCode);
                                    collection.Add(address);
                                    break;
                                }


                            case "BUDOVY":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    int? floors = ParseNullableInt(read(reader, 2));
                                    int addressId = int.Parse(read(reader, 3));
                                    Building building = new Building(id, name, floors, addressId);
                                    collection.Add(building);
                                    break;
                                }


                            case "DIAGNOZY_CISELNIK":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    break;

                                }

                            case "DOKTORI":
                                // Zpracování pro tabulku "DOKTORI"
                                Console.WriteLine("Zpracování tabulky 'DOKTORI'");
                                break;

                            case "LEKY":
                                // Zpracování pro tabulku "LEKY"
                                Console.WriteLine("Zpracování tabulky 'LEKY'");
                                break;

                            case "LUZKA":
                                // Zpracování pro tabulku "LUZKA"
                                Console.WriteLine("Zpracování tabulky 'LUZKA'");
                                break;

                            case "ODDELENI":
                                // Zpracování pro tabulku "ODDELENI"
                                Console.WriteLine("Zpracování tabulky 'ODDELENI'");
                                break;

                            case "PACIENTI":
                                // Zpracování pro tabulku "PACIENTI"
                                Console.WriteLine("Zpracování tabulky 'PACIENTI'");
                                break;

                            case "POJISTOVNY":
                                // Zpracování pro tabulku "POJISTOVNY"
                                Console.WriteLine("Zpracování tabulky 'POJISTOVNY'");
                                break;

                            case "POKOJE":
                                // Zpracování pro tabulku "POKOJE"
                                Console.WriteLine("Zpracování tabulky 'POKOJE'");
                                break;

                            case "POMUCKY":
                                // Zpracování pro tabulku "POMUCKY"
                                Console.WriteLine("Zpracování tabulky 'POMUCKY'");
                                break;

                            case "RECEPTY":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    int docId = int.Parse(read(reader, 1));
                                    int patId = int.Parse(read(reader, 2));
                                    DateTime date = DateTime.Parse(read(reader, 3));
                                    Prescription prescription = new Prescription(id, docId, patId, date);
                                    collection.Add(prescription);
                                    break;
                                }

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
                                    Employee employee = new Employee(id, name, surname, salary, wardId, superiorId, addressId, type);
                                    collection.Add(employee);
                                    break;
                                }

                            default:
                                // Pokud název tabulky neodpovídá žádné z hodnot
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
