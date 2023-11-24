using Nemocnice.Database;
using Nemocnice.ModelObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Nemocnice.Config
{
    public class Launcher
    {
        MainWindow Window { get; set; }
        private DatabaseHandler Handler { get; }

        public Launcher(MainWindow window)
        {
            Window = window;
            Handler = DatabaseHandler.Instance;
        }

        public void Launch()
        {
            Login login = new Login();
            login.WindowStartupLocation = WindowStartupLocation.CenterScreen;   // nastavení dialogu a mainApp aby byly vycentrovany
            Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            login.ShowDialog();

            if (login.DialogResult == true) // úspěšný login/register
                login.Close();
            else  // Uživatel zavřel dialogové okno - ukončí aplikaci
                System.Windows.Application.Current.Shutdown();

            HandleUsersRights();
            FillAppComboBoxes();
            InitUserProfile();
            HandlePacientsRadioButtons();
        }

        private void HandleUsersRights()
        {
            // TODO: v teto metodě dořešit uživatelska prava, možna dle enumu Role (switch Role)
        }

        private void InitUserProfile()
        {
            BitmapImage? img = Handler.LoadLoggedUser(Login.Guest);
            HandleProfileData(Login.Guest, img);
        }

        private void HandleProfileData(bool guest, BitmapImage? img)
        {
            if (!guest)
            {
                Uzivatel? uzivatel = Handler.Uzivatel;
                if (uzivatel != null)
                {
                    Window.profileUserTb.Text = uzivatel.Jmeno;
                    Window.profileRolesCb.Items.Add(uzivatel.Role.ToString());
                    Window.profInsertPictureBtn.IsEnabled = true;
                    Window.profDeletePictureBtn.IsEnabled = true;
                    if (img != null)    // Uživatel nemusí mít nastavený obrázek
                    {
                        Window.profileImg.Source = img;
                        Window.profInsertPictureBtn.Content = "Upravit obrázek";
                    }
                    else
                    {
                        Window.profDeletePictureBtn.IsEnabled = false;
                    }
                }
            }
            else
            {
                Window.profInsertPictureBtn.IsEnabled = false;
                Window.profDeletePictureBtn.IsEnabled = false;
                Window.profileRolesCb.Items.Add("GUEST");
                Window.profileUserTb.Text = "Nepřihlášený";
            }
            Window.profileRolesCb.SelectedIndex = 0;
            Window.profileUserTb.IsReadOnly = true;
            Window.profileRolesCb.IsReadOnly = true;
        }

        private void FillAppComboBoxes()
        {
            Handler.AdminComboBoxHandle(ref Window.comboBox);
            Handler.PacientsComboBoxesHandle(ref Window.skupinyComboBox, ref Window.diagnozyComboBox);
            Handler.RecipeesComboBoxHandle(ref Window.recipeesComboBox);
        }

        private void HandlePacientsRadioButtons()
        {
            Window.diagnozyRadio.Checked += (sender, e) =>
            {
                Window.diagnozyComboBox.IsEnabled = true;
                Window.skupinyComboBox.IsEnabled = false;
            };
            Window.skupinyRadio.Checked += (sender, e) =>
            {
                Window.diagnozyComboBox.IsEnabled = false;
                Window.skupinyComboBox.IsEnabled = true;
            };
        }
        public void ProfInsertPicture_Click()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Obrázky|*.jpg;*.jpeg;*.png";
                DialogResult result = openFileDialog.ShowDialog();
                string selectedFilePath;
                // Po vybrání správného souboru se zpracuje, uloží do databáze a z ní načte do GUI
                // tím je ověřena funkčnost a správnost ukládání binárních dat
                if (result == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    if (Window.profileImg.Source == null)
                    {
                        int savedImgId = Handler.SaveImageToDatabase(selectedFilePath);
                        InitImage(savedImgId);
                    }
                    else
                    {
                        int updatedImgId = Handler.UpdateImageInDatabase(selectedFilePath);
                        InitImage(updatedImgId);
                    }

                }
            }
        }
        public void ProfDeletePicture_Click()
        {
            if (Handler.DeleteCurrentUserImageFromDatabase())
            {
                Window.profileImg.Source = null;
                Window.profDeletePictureBtn.IsEnabled = false;
                Window.profInsertPictureBtn.Content = "Vložte obrázek";
            }
        }

        private void InitImage(int id)
        {
            BitmapImage? bitmap = Handler.LoadImageContentFromDatabase(id);
            if (bitmap != null)
            {
                Window.profileImg.Source = bitmap;
                if (!Window.profDeletePictureBtn.IsEnabled)
                    Window.profDeletePictureBtn.IsEnabled = true;
                Window.profInsertPictureBtn.Content = "Změnit obrázek";
            }
        }

        public void AdminShowTables_Click()
        {
            Handler.AdminShowAllTables(ref Window.comboBox, ref Window.grid);
        }

        public void AdminShowLogs_Click()
        {
            Handler.ShowLogs();
        }
        public void AdminShowKatalog_Click()
        {
            Handler.ShowKatalog();
        }

        public void PacientsShowTable_Click()
        {
            if (Window.skupinyRadio.IsChecked == true)
            {
                Handler.ShowPacients(ref Window.pacientiGrid, ref Window.skupinyComboBox, Ciselnik.SKUPINY);
            }
            else if (Window.diagnozyRadio.IsChecked == true)
            {
                Handler.ShowPacients(ref Window.pacientiGrid, ref Window.diagnozyComboBox, Ciselnik.DIAGNOZY);
            }
        }

        public void RecipeesShowTable_Click()
        {
            Handler.ShowRecipees(ref Window.recipeesComboBox, ref Window.recipeesGrid);
        }
    }
}
