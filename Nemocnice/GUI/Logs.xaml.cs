using Nemocnice.Database;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
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

namespace Nemocnice.GUI
{
    public partial class Logs : Window
    {
        DatabaseConnection databaseConnection;
        OracleConnection Connection { get; set; }

        public Logs()
        {
            databaseConnection = DatabaseConnection.Instance;
            Connection = databaseConnection.OracleConnection;
            InitializeComponent();
            PrintLogs();
        }

        private void PrintLogs()
        {
            using (var command = new OracleCommand("ZiskaniDat", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("p_table_name", OracleDbType.Varchar2).Value = "ZAZNAMY";
                command.Parameters.Add("result_cursor", OracleDbType.RefCursor, ParameterDirection.Output); // Explicitní kurzor? 
                command.ExecuteNonQuery();
                OracleRefCursor refCursor = (OracleRefCursor)command.Parameters["result_cursor"].Value;
                using (OracleDataReader reader = refCursor.GetDataReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    dataGridView.ItemsSource = dataTable.DefaultView;
                }
            }
        }

        private void LogsClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
