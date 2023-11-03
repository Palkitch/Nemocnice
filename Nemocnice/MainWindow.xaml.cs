using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Oracle.ManagedDataAccess.Client;

namespace Nemocnice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OracleConnection connection;
        private List<String> Tables {  get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Tables = new List<String>();
            connection = GetConnection();
            connection.Open();
            ComboBoxHandle("pacienti");
        }

        public static OracleConnection GetConnection()
        {
            string connectionString = "User Id=st67082;" +
                "Password=abcde;" +
                "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=fei-sql3.upceucebny.cz)(PORT=1521))(CONNECT_DATA=(SID=BDAS)(SERVER=DEDICATED)))";
            return new OracleConnection(connectionString);
        }

        public void ComboBoxHandle(string tableName)
        {
            using (var command = new OracleCommand($"SELECT table_name FROM user_tables", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        Tables.Add(name);
                        comboBox.Items.Add(name);
                    }
                    comboBox.SelectedIndex = 0;
                }
            }
        }

        private void printButtonOnAction(object sender, RoutedEventArgs e)
        {
            resultLabel.Content = string.Empty;
            using (var command = new OracleCommand($"SELECT * FROM {comboBox.SelectedValue}", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    foreach(string tableName in Tables)
                    {
                        while (reader.Read())
                        {
                            switch (tableName)
                            {
                                case "pacienti":
                                    // Zpracování pro tabulku "zamestnanci"
                                    Console.WriteLine("Zpracování tabulky 'zamestnanci'");
                                    break;

                                case "doktori":
                                    // Zpracování pro tabulku "nemocnice"
                                    Console.WriteLine("Zpracování tabulky 'nemocnice'");
                                    break;

                                default:
                                    // Pokud název tabulky neodpovídá žádné z hodnot
                                    Console.WriteLine("Neznámá tabulka: " + tableName);
                                    break;
                            }
                        }

                    }

                    try
                        {
                         
                            DataTable dataTable = new DataTable();
                            dataTable.Columns.Add("sss");
                            dataTable.Columns.Add("aaa");
                            dataTable.Rows.Add("a", "a");
                            dataTable.Rows.Add("a", "a");
                            dataTable.Rows.Add("a", "a");
                            grid.ItemsSource = dataTable.DefaultView;


                            string column1 = read(reader, 1);
                            string column2 = read(reader, 2);
                            resultLabel.Content += column1 + "\t" + column2 + "\n";
                        }
                        catch (IndexOutOfRangeException)
                        {
                            resultLabel.Content = "Chybný počet sloupců";
                        }
                }
            }
        }

        private void printTable(List<Object> list) 
        {
            
        }

        private string read(OracleDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? "..." : reader.GetString(columnIndex);
        }
    }
}
