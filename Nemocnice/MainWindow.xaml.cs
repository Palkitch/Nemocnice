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
                                    Diagnosis diagnosis = new Diagnosis(id, name);
                                    collection.Add(diagnosis);  
                                    break;
                                }

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
                                                Employee employee = new Employee(id, name, surName, salary, hospWardId, superiorId, addressId, type);
                                                collection.Add(employee);
                                            }
                                            reader.Close();
                                            break;
                                        }
                                    }
                                }


                            case "LEKY":
                                {
                                    int id = int.Parse(read(reader, 0));
                                    string name = read(reader, 1);
                                    string category = read(reader, 2);
                                    int price = int.Parse(read(reader, 3));
                                    Medicament medicament = new Medicament(id, name, category, price);  
                                    collection.Add(medicament); 
                                    break;
                                }


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
                                // Zpracování pro tabulku "RECEPTY"
                                Console.WriteLine("Zpracování tabulky 'RECEPTY'");
                                break;

                            case "SESTRY":
                                // Zpracování pro tabulku "SESTRY"
                                Console.WriteLine("Zpracování tabulky 'SESTRY'");
                                break;

                            case "ZAMESTNANCI":
                                // Zpracování pro tabulku "ZAMESTNANCI"
                                Console.WriteLine("Zpracování tabulky 'ZAMESTNANCI'");
                                break;

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
