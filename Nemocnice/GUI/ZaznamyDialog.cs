using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.OracleClient;
using System.Windows.Forms;

namespace Nemocnice.GUI
{
    internal class ZaznamyDialog : Form
    {
        private DataGridView dataGridView;
        private Button closeButton;
        private OracleDataReader reader;

        public ZaznamyDialog(OracleDataReader reader)
        {
            this.reader = reader;
            dataGridView = new DataGridView();
            closeButton = new Button();
            InitializeComponents();
            VypisDat();
        }

        private void InitializeComponents()
        {
            Text = "Záznamy";
            Size = new System.Drawing.Size(600, 500);

            dataGridView.Dock = DockStyle.Fill;
            dataGridView.ReadOnly = true; // Nastavení jako pouze pro čtení
            dataGridView.AllowUserToAddRows = false; // Skryje prázdný řádek pro přidání nových dat

            closeButton.Text = "Zavřít";
            closeButton.Dock = DockStyle.Bottom;
            closeButton.Click += CloseButton_Click;

            Controls.Add(dataGridView);
            Controls.Add(closeButton);
        }

        private void VypisDat()
        {
            try
            {
                var dataTable = new System.Data.DataTable();
                dataTable.Load(reader);
                dataGridView.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při čtení dat: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                reader.Close();
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
