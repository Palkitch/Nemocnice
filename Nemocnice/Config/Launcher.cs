using Microsoft.VisualBasic.ApplicationServices;
using Nemocnice.Database;
using Nemocnice.ModelObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MessageBox = System.Windows.MessageBox;

namespace Nemocnice.Config
{
	public class Launcher
	{
		MainWindow Window { get; set; }
		private DatabaseHandler Handler { get; }
		private List<Uzivatel> Users { get; set; }
		private Uzivatel? SelectedUser { get; set; }

		public Launcher(MainWindow window)
		{
			Window = window;
			Handler = DatabaseHandler.Instance;
			Users = new List<Uzivatel>();
			SelectedUser = null;
		}

		public void Launch()
		{
            HandleLogin();
            FillAppComboBoxes();
            HandleCurrentlyLoggedUserRights();
            HandlePacientsRadioButtons();
            InitUserProfile();
			InitUsers();
		}

        private void HandleLogin() 
        {
            Login login = new Login();
            login.WindowStartupLocation = WindowStartupLocation.CenterScreen;   // nastavení dialogu a mainApp aby byly vycentrovany
            Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            login.ShowDialog();

            if (login.DialogResult == true) // úspěšný login/register
                login.Close();
            else  // Uživatel zavřel dialogové okno - ukončí aplikaci
                System.Windows.Application.Current.Shutdown();
        }

        private void HandleCurrentlyLoggedUserRights()
		{
			if (Login.Guest) 
			{
                Window.adminTabItem.Visibility = Visibility.Hidden;
                Window.usersTabItem.Visibility = Visibility.Hidden;
				
            } else if (Handler.Uzivatel != null)
			{
                if (Handler.Uzivatel.Role == Role.SESTRA || Handler.Uzivatel.Role == Role.DOKTOR)
                {
                    Window.adminTabItem.Visibility = Visibility.Hidden;
                    Window.usersTabItem.Visibility = Visibility.Hidden;
                    Window.employeesTabItem.Visibility = Visibility.Hidden;
                }
                else if (Handler.Uzivatel.Role == Role.PRIMAR) 
                {
                    Window.profileEmulationCb.Visibility = Visibility.Visible;
                    Window.profileEmulationLabel.Visibility = Visibility.Visible;
                    Window.adminTabItem.Visibility = Visibility.Visible;
                    Window.usersTabItem.Visibility = Visibility.Visible; 
                    Window.employeesTabItem.Visibility = Visibility.Visible;
                }
            }
        }

        private void FillAppComboBoxes()
		{
			Handler.AdminComboBoxHandle(ref Window.comboBox);
			Handler.PacientsComboBoxesHandle(ref Window.skupinyComboBox, ref Window.diagnozyComboBox);
			Handler.RecipeesComboBoxHandle(ref Window.recipeesComboBox);
            Handler.ScheduleComboBoxHandle(ref Window.scheduleNurseCb);
            InitEmulationComboBox();
        }

        #region TabItem: Uživatelé

        private void InitUsers()
        {
            RefreshUsers();
        }

        private void RefreshUsers()
        {
            Users = Handler.LoadAllUsers();
            ObservableCollection<Uzivatel> collection = new ObservableCollection<Uzivatel>();
            foreach (Uzivatel user in Users)
            {
                collection.Add(user);
            }
            Window.usersGrid.ItemsSource = Users;
            Window.usersRoleCb.ItemsSource = Enum.GetValues(typeof(Role)).Cast<Role>().Select(role => role.ToString()).ToList();
        }

        public void ChangeUsersValues()	// při překlikávání uživatelů se mění hodnoty komponent na stránce
        {
            // Získání vybrané položky z DataGridu
            SelectedUser = (Uzivatel)Window.usersGrid.SelectedItem;

            if (SelectedUser != null)
            {
                Window.usersUsernameTb.Text = SelectedUser.Jmeno;
                switch (SelectedUser.Role)
                {
                    case Role.PRIMAR: Window.usersRoleCb.SelectedIndex = 0; break;
                    case Role.DOKTOR: Window.usersRoleCb.SelectedIndex = 1; break;
                    case Role.SESTRA: Window.usersRoleCb.SelectedIndex = 2; break;
                }
            }
        }

        public void UpdateUserFromAdminTab()
        {
            string newName = Window.usersUsernameTb.Text;
            string? roleText = Window.usersRoleCb.SelectedItem.ToString();
            string? oldName = SelectedUser.Jmeno;

            if (newName != null && roleText != null && oldName != null)
            {
                Handler.UpdateUserFromAdminTab(oldName, newName, roleText);
            }
            RefreshUsers();
        }

        public void DeleteUser()
        {
            string userName = Window.usersUsernameTb.Text;
            string? roleText = Window.usersRoleCb.SelectedItem.ToString();

            if (userName != null && Handler.Uzivatel != null) 
            {
                if (Handler.Uzivatel.Jmeno == userName)
                {
                    MessageBox.Show("Sám sebe není možno mazat", "Varování");
                }
                else if (Handler.Uzivatel.Role.ToString() == roleText) 
                {
                    MessageBox.Show("Není možno mazat uživatele s rolí PRIMAR", "Varování");
                }
                else
                {
                    Handler.DeleteUserFromAdminTab(userName);
                }
            }
            RefreshUsers();
        }
        #endregion

        #region TabItem: Profil

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

        private void InitEmulationComboBox()
        {
            Window.profileEmulationCb.ItemsSource = Enum.GetValues(typeof(Role)).Cast<Role>().Select(role => role.ToString()).ToList();
        }

        public void HandleEmulation()
        {
            if (Handler.Uzivatel != null)
            {
                string? roleText = Window.profileEmulationCb.SelectedItem.ToString();
                if (roleText != null)
                {
                    Role selectedRole = (Role)Enum.Parse(typeof(Role), roleText);
                    Handler.Uzivatel.Role = selectedRole;
                    HandleCurrentlyLoggedUserRights();
                }
                else 
                {
                    MessageBox.Show("Nelze zjistit vybranou roli", "Chyba");
                }
            }
        }
        #endregion

        #region TabItem: Pacienti

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
            Window.allRadio.Checked += (sender, e) =>
            {
                Window.diagnozyComboBox.IsEnabled = false;
                Window.skupinyComboBox.IsEnabled = false;
            };

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
            else if (Window.allRadio.IsChecked == true)
            {
                Handler.ShowPacients(ref Window.pacientiGrid, ref Window.diagnozyComboBox, Ciselnik.NONE);
            }
        }

        public void AddPacient_Click()
        {
            Handler.AddPacient(ref Window.pacientiGrid);
        }
        #endregion

        #region TabItem: Admin

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
        #endregion

        #region TabItem: Recepty
        public void RecipeesShowTable_Click()
        {
            Handler.ShowRecipees(ref Window.recipeesComboBox, ref Window.recipeesGrid);
        }

        #endregion

        #region TabItem: Rozpis
        public void ShowSchedule()
        {
            Handler.ShowShedule(ref Window.scheduleNurseCb, ref Window.scheduleGrid);
        }
        #endregion

        #region TabItem: Zamestnanci

        public void ShowEmployees()
        {
            Handler.ShowEmployees(ref Window.employeesGrid);
        }

        public void EditEmployee()
        {
            // TODO: přidat do database handleru metodu pro edit employee
        }

        public void AddEmployee()
        {
            // TODO: přidat do database handleru metodu pro add employee
        }
        #endregion

    }
}
