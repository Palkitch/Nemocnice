using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.OracleClient;
using System.Windows.Forms;

namespace Nemocnice.Database
{
    internal class ZaznamyDialog : Form
    {
        private DataGridView dataGridView;
        private Button closeButton;
        private OracleDataReader reader;

        public ZaznamyDialog(OracleDataReader reader)
        {
            this.reader = reader;
            InitializeComponents();
            VypisDat();
        }

        private void InitializeComponents()
        {
            this.Text = "Záznamy";
            this.Size = new System.Drawing.Size(500, 400);

            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.ReadOnly = true; // Nastavení jako pouze pro čtení
            dataGridView.AllowUserToAddRows = false; // Skryje prázdný řádek pro přidání nových dat

            closeButton = new Button();
            closeButton.Text = "Zavřít";
            closeButton.Dock = DockStyle.Bottom;
            closeButton.Click += CloseButton_Click;

            this.Controls.Add(dataGridView);
            this.Controls.Add(closeButton);
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
            this.Close();
        }
    }
}
