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
            StartupInit();
        }

        private void StartupInit()
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
            handler.AdminComboBoxHandle(ref comboBox);
            handler.PacientiComboBoxyHandle(ref skupinyComboBox, ref diagnozyComboBox);
            handler.LoadLoggedUser(profileUserTb, profileRolesCb, profileImg,
            profInsertPictureBtn, profDeletePictureBtn, Login.Guest);
            diagnozyRadio.Checked += (sender, e) =>
            {
                diagnozyComboBox.IsEnabled = true;
                skupinyComboBox.IsEnabled = false;
            };
            skupinyRadio.Checked += (sender, e) =>
            {
                diagnozyComboBox.IsEnabled = false;
                skupinyComboBox.IsEnabled = true;
            };
        }

        private void printButtonOnAction(object sender, RoutedEventArgs e)
        {
            handler.LoadDataFromTable(ref comboBox, ref grid);
        }

        private void ProfInsertPicture_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Obrázky|*.jpg;*.jpeg;*.png";
                DialogResult result = openFileDialog.ShowDialog();
                string selectedFilePath;
                // Po vybrání správného souboru se zpracuje, uloží do databáze a z ní načte do GUI
                // tím je ověřena funkčnost a správnost ukládání binárních dat
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    if (profileImg.Source == null)
                    {
                        int savedImgId = handler.SaveImageToDatabase(selectedFilePath);
                        InitImage(savedImgId);
                    }
                    else
                    {
                        int updatedImgId = handler.UpdateImageInDatabase(selectedFilePath);
                        InitImage(updatedImgId);
                    }

                }
            }
        }

        private void InitImage(int id)
        {
            BitmapImage? bitmap = handler.LoadImageContentFromDatabase(id);
            if (bitmap != null)
            {
                profileImg.Source = bitmap;
                if (!profDeletePictureBtn.IsEnabled)
                    profDeletePictureBtn.IsEnabled = true;
                profInsertPictureBtn.Content = "Změnit obrázek";
            }
        }

        private void ProfDeletePicture_Click(object sender, RoutedEventArgs e)
        {
            if (handler.DeleteCurrentUserImageFromDatabase())
            {
                profileImg.Source = null;
                profDeletePictureBtn.IsEnabled = false;
            }
        }

        private void ZaznamButtonOnAction(object sender, RoutedEventArgs e)
        {
            handler.VypisZaznamu();
        }

        private void ZobrazitCiselnikOnAction(object sender, RoutedEventArgs e)
        {
            if (skupinyRadio.IsChecked == true)
            {
                handler.VypisPacientu(ref pacientiGrid, ref skupinyComboBox, Ciselnik.SKUPINY);
            }
            else if (diagnozyRadio.IsChecked == true)
            {
                handler.VypisPacientu(ref pacientiGrid, ref diagnozyComboBox, Ciselnik.DIAGNOZY);
            }
        }

    }
}
