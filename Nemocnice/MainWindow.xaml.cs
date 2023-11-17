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
using MessageBox = System.Windows.MessageBox;

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
            handler = DatabaseHandler.Instance;

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

            // main okno appky bude zobrazeno vycentrované na obrazovce
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // naplnění comboboxu pro admina aby si mohl zobrazit každou tabulku
            handler.adminComboBoxHandle(ref comboBox);
            if (Login.Guest == false) 
            {
                handler.loadLoggedUser(profileUserTb, profileRolesCb, profileImg);
            }
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
                DialogResult result = openFileDialog.ShowDialog();

                // Po vybrání správného souboru se zpracuje, uloží do databáze a z ní načte do GUI
                // tím je ověřena funkčnost a správnost ukládání binárních dat
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    int lastId = handler.saveImageToDatabase(selectedFilePath);
                    BitmapImage bitmap = handler.loadImageFromDatabase(lastId);
                    if (bitmap != null) 
                    {
                        profileImg.Source = bitmap;
                        profileInsertPictureButton.Content = "Změňte obrázek";
                    }
                }
            }
        }
    }
}
