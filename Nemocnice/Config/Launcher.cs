﻿using Microsoft.VisualBasic.ApplicationServices;
using Nemocnice.Database;
using Nemocnice.GUI.Dialogs;
using Nemocnice.Model;
using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Nemocnice.Config
{
	public class Launcher
	{
		MainWindow Window { get; set; }
		private DatabaseHandler Handler { get; }
		private List<Uzivatel> Users { get; set; }
		private Uzivatel? SelectedUser { get; set; }
		private DatabaseConnection DatabaseConnection { get; }
		private OracleConnection Connection { get; }

		public Launcher(MainWindow window)
		{
			DatabaseConnection = DatabaseConnection.Instance;
			Connection = DatabaseConnection.OracleConnection;
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
				Window.requestsTabItem.Visibility = Visibility.Hidden;
				Window.pacientsAdd.IsEnabled = false;
				Window.pacientsEdit.IsEnabled = false;
				Window.employeesTabItem.Visibility = Visibility.Hidden;

			}
			else if (Handler.Uzivatel != null)
			{
				if (Handler.Uzivatel.Role == Role.SESTRA || Handler.Uzivatel.Role == Role.DOKTOR)
				{
					Window.adminTabItem.Visibility = Visibility.Hidden;
					Window.usersTabItem.Visibility = Visibility.Hidden;
					Window.employeesTabItem.Visibility = Visibility.Hidden;
					Window.requestsTabItem.Visibility = Visibility.Hidden;
					Window.employeesTabItem.Visibility = Visibility.Hidden;
				}
				else if (Handler.Uzivatel.Role == Role.PRIMAR)
				{
					Window.profileEmulationCb.Visibility = Visibility.Visible;
					Window.profileEmulationLabel.Visibility = Visibility.Visible;
					Window.adminTabItem.Visibility = Visibility.Visible;
					Window.usersTabItem.Visibility = Visibility.Visible;
					Window.employeesTabItem.Visibility = Visibility.Visible;
					Window.employeesTabItem.Visibility = Visibility.Visible;
					Window.requestsTabItem.Visibility = Visibility.Visible;
				}
			}
		}

		private void FillAppComboBoxes()
		{
			Handler.AdminComboBoxHandle(ref Window.adminCb);
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

		public void ChangeUsersValues() // při překlikávání uživatelů se mění hodnoty komponent na stránce
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
		public void EditPacient_Click()
		{
			Handler.EditPacient(ref Window.pacientiGrid);
		}

		public void PacientsFindPacient()
		{
			Handler.FindPacient(ref Window.pacientiGrid, ref Window.pacientsPacientTb);
		}

		#endregion

		#region TabItem: Admin

		public void AdminShowTables_Click()
		{
			Handler.AdminShowAllTables(ref Window.adminCb, ref Window.adminGrid);
		}

		public void AdminShowLogs_Click()
		{
			Handler.ShowLogs();
		}
		public void AdminShowKatalog_Click()
		{
			Handler.ShowKatalog();
		}

		public void AdminEditRow(DataGridCellEditEndingEventArgs cell)
		{
			TextBox editedTextBox = cell.EditingElement as TextBox;

			// Získání textové hodnoty editované buňky
			string newValue = editedTextBox?.Text ?? "";

			// Získání editované hodnoty
			string objectProperty = cell.Column.Header?.ToString() ?? "";

			// Získání původní hodnoty buňky pro konkrétní sloupec
			var modelObject = cell.Row.Item;
			Type objectType = modelObject.GetType();
			string objectName = objectType.Name;

			Handler.UpdateAdminTable(modelObject, objectProperty, newValue);
		}

		public void AddDataClick()
		{
			switch (Window.adminCb.Text)
			{
				case "Adresy":
					{
						AdresaDialog adresaDialog = new AdresaDialog();
						adresaDialog.ShowDialog();
						break;
					}
				case "Budovy":
					{
						BudovaDialog budovaDialog = new BudovaDialog();
						budovaDialog.ShowDialog();
						break;
					}
				case "Diagnózy":
					{
						DiagnozaDialog diagnozaDialog = new DiagnozaDialog();
						diagnozaDialog.ShowDialog();
						break;
					}

				case "Léky":
					{
						LekyDialog lekyDialog = new LekyDialog();
						lekyDialog.ShowDialog();
						break;
					}

				case "Pomůcky":
					{
						PomuckyDialog pomuckyDialog = new PomuckyDialog();
						pomuckyDialog.ShowDialog();
						break;
					}

				case "Zaměstnanci":
				case "Sestry":
					{
						ZamestnanciDialog zamestnanciDialog = new ZamestnanciDialog("s");
						zamestnanciDialog.ShowDialog();
						break;
					}
				case "Doktoři":
					{
						ZamestnanciDialog zamestnanciDialog = new ZamestnanciDialog("d");
						zamestnanciDialog.ShowDialog();
						break;
					}

				case "Lůžka":
					{
						LuzkaDialog luzkaDialog = new LuzkaDialog();
						luzkaDialog.ShowDialog();
						break;
					}

				case "Pacienti":
					{
						PacientDialog pacientDialog = new PacientDialog(Handler.Uzivatel);
						pacientDialog.ShowDialog();
						break;
					}

				case "Pojišťovny":
					{
						PojistovnyDialog pojistovnyDialog = new PojistovnyDialog();
						pojistovnyDialog.ShowDialog();
						break;
					}

				case "Oddělení":
					{
						OddeleniDialog oddeleniDialog = new OddeleniDialog();
						oddeleniDialog.ShowDialog();
						break;
					}

				case "Pokoje":
					{
						PokojeDialog pokojeDialog = new PokojeDialog();
						pokojeDialog.ShowDialog();
						break;
					}

			}
			Handler.AdminShowAllTables(ref Window.adminCb, ref Window.adminGrid);
		}


		public void DeleteTableClick()
		{
			string nazevTabulky = Window.adminCb.Text.Trim();
			Handler.DeleteAdminTable(nazevTabulky, ref Window.adminGrid, ref Window.adminCb);
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

		#region TabItem: Zadosti
		public void RequestsDenyRequest_Click()
		{
			if (Window.requestsGrid.SelectedItem != null)
			{
				DataRowView? selectedItem = Window.requestsGrid.SelectedItem as DataRowView;
				if (selectedItem != null)
				{
					int id = Convert.ToInt32(selectedItem.Row[0]);
					Handler.DenyRequest(ref Window.requestsGrid, id);
				}
				else
				{
					MessageBox.Show("Vyber nějaký řádek!", "Chyba");
				}
			}
		}

		public void RequestsShowRequests_Click()
		{
			Handler.ShowRequests(ref Window.requestsGrid);

		}

		public void RequestsAcceptRequest_Click()
		{
			if (Window.requestsGrid.SelectedItem != null)
			{
				DataRowView? selectedItem = Window.requestsGrid.SelectedItem as DataRowView;
				if (selectedItem != null)
				{
					int id = Convert.ToInt32(selectedItem.Row[0]);
					bool accepted = Handler.AcceptRequest(ref Window.requestsGrid, id);
					if (accepted)
					{
						MessageBox.Show("Žádost úspěšně schválena!", "Souhlas");
					}
					else
					{
						MessageBox.Show("Žádost nebyla schválena!", "Chyba");
					}
				}
				else
				{
					MessageBox.Show("Vyber nějaký řádek!", "Chyba");
				}
			}
		}
		#endregion
	}
}
