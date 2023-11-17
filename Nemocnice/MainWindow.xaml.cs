using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Controls;
using System.Windows.Data;
using Nemocnice.Database;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace Nemocnice
{
    public partial class MainWindow : Window
    {
        private DatabaseHandler handler;

        public MainWindow()
        {
            InitializeComponent();
            startupInit();
        }

        private void startupInit()
        {
            // handler init
            handler = new DatabaseHandler();

            // Login setup
            Login login = new Login();
            login.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            login.ShowDialog();
            if (login.DialogResult == true) // úspěšný login/register
            {
                login.Close();
            }
            else  // Uživatel zavřel dialogové okno - ukončí aplikaci
            {
                System.Windows.Application.Current.Shutdown();
            }

            // mainwindow center possition
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;


            // mainwindow combobox init
            handler.ComboBoxHandle(ref comboBox);
        }

        private void printButtonOnAction(object sender, RoutedEventArgs e)
        {
            handler.SwitchMethod(ref comboBox, ref grid);
        }

        private void profileInsertPictureButton_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Obrázky|*.jpg;*.jpeg;*.png|Všechny soubory|*.*";

                // Nastavíme počáteční adresář pro prohlížeč souborů (volitelné)
                openFileDialog.InitialDirectory = "C:\\";

                // Zobrazíme dialog pro výběr souboru
                DialogResult result = openFileDialog.ShowDialog();

                // Pokud uživatel vybere soubor a potvrdí dialog, zpracujeme vybraný soubor
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    System.Windows.MessageBox.Show("Načteno: " + selectedFilePath);

                    handler.saveImageToDatabase(selectedFilePath);

                    profileImg.Source = handler.loadImageFromDatabase();
                }
            }
        }
    }
}
