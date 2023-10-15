using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
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
                    while (reader.Read())
                    {
                        try
                        {
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
        }
        private string read(OracleDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? "..." : reader.GetString(columnIndex);
        }
    }
}
