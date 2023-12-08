using Microsoft.VisualBasic;
using Nemocnice.Database;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
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
			// Clear the existing items and add the new items
			comboBox.Items.Clear();
			comboBox.Items.Add("tabulky");
			comboBox.Items.Add("procedury");
			comboBox.Items.Add("funkce");
			comboBox.Items.Add("triggery");
			comboBox.Items.Add("sekvence");

			// Set the default selected item
			comboBox.SelectedIndex = 0;

			// Attach the event handler for selected index change
			comboBox.SelectionChanged += ComboBox_SelectedIndexChanged;
		}

		private void ComboBox_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
		{
			string selectedOption = comboBox.SelectedItem.ToString();

			// Call the appropriate method based on the selected object PacientDialogType
			switch (selectedOption)
			{
				case "tabulky":
					PrintCatalog("TABLE");
					break;
				case "procedury":
					PrintCatalog("PROCEDURE");
					break;
				case "funkce":
					PrintCatalog("FUNCTION");
					break;
				case "triggery":
					PrintCatalog("TRIGGER");
					break;
				case "sekvence":
					PrintCatalog("SEQUENCE");
					break;
				default:
					MessageBox.Show("Invalid option selected");
					break;
			}
		}


		private void PrintCatalog(string objectType)
		{
			try
			{
				using (OracleCommand command = new OracleCommand("VypisKatalog", Connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Add parameters
					command.Parameters.Add("p_object_type", OracleDbType.Varchar2).Value = objectType;
					command.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

					// Execute the stored procedure
					command.ExecuteNonQuery();

					// Retrieve the cursor from the output parameter
					OracleRefCursor cursor = (OracleRefCursor)command.Parameters["p_cursor"].Value;

					// Read data from the cursor
					using (OracleDataReader reader = cursor.GetDataReader())
					{
						DataTable dataTable = new DataTable();

						// Clear existing columns
						dataTable.Columns.Clear();

						// Add a new column
						dataTable.Columns.Add("Object_Name", typeof(string));

						// Load data into the DataTable
						while (reader.Read())
						{
							DataRow row = dataTable.NewRow();
							row["Object_Name"] = reader["object_name"].ToString();
							dataTable.Rows.Add(row);
						}

						// Set the DataTable as the data source
						dataGridView.ItemsSource = dataTable.DefaultView;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error printing catalog: {ex.Message}", "Error");
			}
		}




		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
