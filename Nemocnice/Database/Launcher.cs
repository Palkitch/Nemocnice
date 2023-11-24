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

namespace Nemocnice.Database
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

            FillAppComboBoxes();
            InitUserProfile();
            HandlePacientsRadioButtons();
        }

        private void InitUserProfile()
        {
            Handler.LoadLoggedUser(Window.profileUserTb, Window.profileRolesCb, Window.profileImg,
            Window.profInsertPictureBtn, Window.profDeletePictureBtn, Login.Guest);
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
                if (result == System.Windows.Forms.DialogResult.OK)
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
            Handler.LoadDataFromTable(ref Window.comboBox, ref Window.grid);
        }

        public void AdminShowLogs_Click()
        {
            Handler.VypisZaznamu();
        }

        public void PacientsShowTable_Click()
        {
            if (Window.skupinyRadio.IsChecked == true)
            {
                Handler.VypisPacientu(ref Window.pacientiGrid, ref Window.skupinyComboBox, Ciselnik.SKUPINY);
            }
            else if (Window.diagnozyRadio.IsChecked == true)
            {
                Handler.VypisPacientu(ref Window.pacientiGrid, ref Window.diagnozyComboBox, Ciselnik.DIAGNOZY);
            }
        }

        public void RecipeesShowTable_Click()
        {
            Handler.ShowRecipees(ref Window.recipeesComboBox, ref Window.recipeesGrid);
        }
    }
}
