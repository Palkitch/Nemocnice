using Microsoft.VisualBasic;
using Nemocnice.Database;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace Nemocnice.GUI
{
    public partial class Catalog : Window
    {
        DatabaseConnection databaseConnection;
        OracleConnection Connection { get; set; }

        public Catalog()
        {
            databaseConnection = DatabaseConnection.Instance;
            Connection = databaseConnection.OracleConnection;
            InitializeComponent();
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            try
            {
                using (OracleCommand command = new OracleCommand("VypisTabulekKatalog", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor);
                    cursorParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(cursorParam);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox.Items.Add(reader["table_name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání názvů tabulek: " + ex.Message, "Chyba");
            }
            comboBox.SelectedIndex = 0;
        }

        private void ComboBox_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            string? selectedTable = comboBox.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(selectedTable))
            {
                PrintCatalog(selectedTable);
            }
        }

        private void PrintCatalog(string nazevTabulky) 
        {
            try
            {
                using (OracleCommand command = new OracleCommand("VypisSloupcuKatalog", Connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter tableParam = new OracleParameter("p_table_name", OracleDbType.Varchar2);
                    tableParam.Direction = ParameterDirection.Input;
                    tableParam.Value = nazevTabulky;
                    command.Parameters.Add(tableParam);

                    OracleParameter cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor);
                    cursorParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(cursorParam);

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        dataGridView.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání sloupců tabulky: " + ex.Message, "Chyba");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
