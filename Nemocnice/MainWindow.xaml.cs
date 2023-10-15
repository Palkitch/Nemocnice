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
        public MainWindow()
        {
            InitializeComponent();
            OracleConnection connection = GetConnection();
            connection.Open();
            PrintNameColumnValues(connection, "pacienti");
        }
        public static OracleConnection GetConnection()
        {
            string connectionString = "User Id=st67082;" +
                "Password=abcde;" +
                "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=fei-sql3.upceucebny.cz)(PORT=1521))(CONNECT_DATA=(SID=BDAS)(SERVER=DEDICATED)))";
            return new OracleConnection(connectionString);
        }
        public void PrintNameColumnValues(OracleConnection connection, string tableName)
        {
            using (var command = new OracleCommand($"SELECT jmeno FROM {tableName} WHERE id_doktor = 10 ORDER BY id_pacient ", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        //Console.WriteLine("Name: " + name);
                        label123.Content += name + "\n";
                    }
                }
            }
        }
    }
}
