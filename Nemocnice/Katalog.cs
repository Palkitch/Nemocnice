using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Windows.Forms;

namespace Nemocnice
{
    internal class Katalog : Form
    {
        private ComboBox comboBox = new ComboBox();
        private DataGridView dataGridView = new DataGridView();
        private Button closeButton = new Button();
        private OracleConnection connection;

        public Katalog(OracleConnection connection)
        {
            this.connection = connection;
            InitializeComponents();
            InitializeComboBox();
        }

        private void InitializeComponents()
        {
            Text = "Záznamy";
            Size = new System.Drawing.Size(400, 300);

            comboBox.Dock = DockStyle.Top;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;

            dataGridView.Dock = DockStyle.Fill;
            dataGridView.ReadOnly = true;
            dataGridView.AllowUserToAddRows = false;

            closeButton.Text = "Zavřít";
            closeButton.Dock = DockStyle.Bottom;
            closeButton.Click += CloseButton_Click;

            Controls.Add(comboBox);
            Controls.Add(dataGridView);
            Controls.Add(closeButton);
        }

        private void InitializeComboBox()
        {
            try
            {
                using (OracleCommand command = new OracleCommand("VypisTabulekKatalog", connection))
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
                MessageBox.Show("Chyba při načítání názvů tabulek: " + ex.Message, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            comboBox.SelectedIndex = 0;
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = comboBox.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(selectedTable))
            {
                VypisSloupcu(selectedTable);
            }
        }

        private void VypisSloupcu(string nazevTabulky)
        {
            try
            {
                using (OracleCommand command = new OracleCommand("VypisSloupcuKatalog", connection))
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
                        dataGridView.DataSource = null;
                        DataTable dataTable = new DataTable();

                        dataTable.Load(reader);
                        dataGridView.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání sloupců tabulky: " + ex.Message, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            dataGridView.Columns[1].Width = 200;
        }



        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
