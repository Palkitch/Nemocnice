using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Nemocnice.GUI;
using Nemocnice.Model;
using Nemocnice.ModelObjects;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Nemocnice.Database
{
	public class DatabaseHandler
	{
		private Dictionary<string, string> TableAliasMapping { get; set; }
		private DatabaseConnection DatabaseConnection { get; }
		public OracleConnection Connection { get; }
		public Uzivatel? Uzivatel { get; set; }
		public List<Diagnoza> Diagnozy { get; set; }
		public List<KrevniSkupina> KrevniSkupiny { get; set; }

		private static DatabaseHandler? instance;

		public static DatabaseHandler Instance  // singleton kvuli otevirani connection v konstruktoru
		{
			get
			{
				instance ??= new DatabaseHandler(); // ??= přiřazeni hodnoty pouze je proměnná null
				return instance;
			}
		}

		public DatabaseHandler()
		{
			DatabaseConnection = DatabaseConnection.Instance;
			Diagnozy = new List<Diagnoza>();
			KrevniSkupiny = new List<KrevniSkupina>();
			TableAliasMapping = new Dictionary<string, string>();
			Connection = DatabaseConnection.OracleConnection;
			try { Connection.Open(); }
			catch
			{
				MessageBox.Show("ZAPNI SI VPN", "Chyba");
				Environment.Exit(444);
			}
		}

		#region Login/Register
		public string? Login(string username, string password)
		{
			if (!UserExists(username))
			{
				MessageBox.Show($"Uživatel s jménem {username} neexistuje.", "Upozornění");
				return null;
			}

			string? storedHashedPassword = GetHashedPassword(username);
			return storedHashedPassword == password ? storedHashedPassword : null;
		}

		public bool Register(string username, string password)
		{
			if (UserExists(username))
			{
				MessageBox.Show($"Uživatel s jménem {username} již existuje.", "Upozornění");
				return false;
			}
			string insertQuery = "INSERT INTO Uzivatele (nazev, role, heslo) VALUES (:uname, :urole, :pwd)";
			OracleCommand cmd = new OracleCommand(insertQuery, Connection);
			cmd.Parameters.Add(new OracleParameter("uname", username));
			cmd.Parameters.Add(new OracleParameter("urole", Role.SESTRA.ToString()));
			cmd.Parameters.Add(new OracleParameter("pwd", password));
			cmd.ExecuteNonQuery();
			Uzivatel = new Uzivatel(username, Role.SESTRA);
			return true;
		}

		private bool UserExists(string username)
		{
			string query = "SELECT COUNT(*) FROM Uzivatele WHERE nazev = :uname";
			OracleCommand cmd = new OracleCommand(query, Connection);
			cmd.Parameters.Add(new OracleParameter("uname", username));
			int count = Convert.ToInt32(cmd.ExecuteScalar());
			return count > 0;
		}

		private string? GetHashedPassword(string username)
		{
			string? heslo = null;
			string sql = "SELECT heslo, role FROM Uzivatele WHERE nazev = :uname";
			using (var cmd = new OracleCommand(sql, Connection))
			{
				cmd.Parameters.Add(new OracleParameter("uname", username));
				using (var cmdReader = cmd.ExecuteReader())
				{
					while (cmdReader.Read())
					{
						heslo = ReadString(cmdReader, 0);
						string roleString = ReadString(cmdReader, 1);
						if (Enum.TryParse<Role>(roleString, out Role role))
						{
							Uzivatel = new Uzivatel(username, role);
						}
					}
					cmdReader.Close();
					return heslo;
				}
			}
		}
		#endregion

		#region TabItem: ADMIN
		public void AdminComboBoxHandle(ref ComboBox comboBox)
		{
			{
				using (OracleCommand command = new OracleCommand("ZobrazeniTabulek", Connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.Parameters.Add("result_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

					command.ExecuteNonQuery();

					OracleDataReader reader = ((OracleRefCursor)command.Parameters["result_cursor"].Value).GetDataReader();
					while (reader.Read())
					{
						string? aliasTableName = reader["alias_table_name"].ToString();
						string? tableName = reader["table_name"].ToString();
						if (aliasTableName != null && tableName != null)
							TableAliasMapping.Add(tableName, aliasTableName);
					}
				}
				foreach (string value in TableAliasMapping.Values)
				{
					comboBox.Items.Add(value);
				}
				comboBox.SelectedIndex = 0;
			}
		}

		public void AdminShowAllTables(ref ComboBox adminComboBox, ref DataGrid adminGrid)
		{

			ObservableCollection<object> collection = new ObservableCollection<object>();

			string? selectedTableAlias = adminComboBox.SelectedItem.ToString();
			string? dictValue = TableAliasMapping.FirstOrDefault(x => x.Value == selectedTableAlias).Key;


			using (var command = new OracleCommand("ZiskaniDat", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;

				// Převedení názvu tabulky na velká písmena
				string? tableName = dictValue.ToUpper();


				// Přidání parametrů pro volání procedury
				command.Parameters.Add("p_table_name", OracleDbType.Varchar2).Value = dictValue;
				command.Parameters.Add("result_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

				// Spuštění příkazu
				command.ExecuteNonQuery();
				OracleRefCursor refCursor = (OracleRefCursor)command.Parameters["result_cursor"].Value;

				if (dictValue == "DOKTORI")
				{
					string? sql = "SELECT z.* FROM ZAMESTNANCI z JOIN DOKTORI d ON z.id_zamestnanec = d.id_zamestnanec";
					using (var cmd = new OracleCommand(sql, Connection))
					{
						using (var cmdReader = cmd.ExecuteReader())
						{
							while (cmdReader.Read())
							{
								int id = int.Parse(ReadString(cmdReader, 0));
								string? name = ReadString(cmdReader, 1);
								string? surName = ReadString(cmdReader, 2);
								int salary = int.Parse(ReadString(cmdReader, 3));
								int hospWardId = int.Parse(ReadString(cmdReader, 4));
								int? superiorId = ParseNullableInt(ReadString(cmdReader, 5));
								int addressId = int.Parse(ReadString(cmdReader, 6));
								char type = ReadString(cmdReader, 7)[0];
								Zamestnanec employee = new Zamestnanec(id, name, surName, salary, hospWardId, superiorId, addressId, type);
								collection.Add(employee);
							}
							cmdReader.Close();
						}
					}
				}
				else if (dictValue == "SESTRY")
				{
					string? sql = "SELECT z.* FROM ZAMESTNANCI z JOIN SESTRY s ON z.id_zamestnanec = s.id_zamestnanec";
					using (var cmd = new OracleCommand(sql, Connection))
					{
						using (var cmdReader = cmd.ExecuteReader())
						{
							while (cmdReader.Read())
							{
								int id = int.Parse(ReadString(cmdReader, 0));
								string? name = ReadString(cmdReader, 1);
								string? surName = ReadString(cmdReader, 2);
								int salary = int.Parse(ReadString(cmdReader, 3));
								int hospWardId = int.Parse(ReadString(cmdReader, 4));
								int? superiorId = ParseNullableInt(ReadString(cmdReader, 5));
								int addressId = int.Parse(ReadString(cmdReader, 6));
								char type = ReadString(cmdReader, 7)[0];
								Zamestnanec employee = new Zamestnanec(id, name, surName, salary, hospWardId, superiorId, addressId, type);
								collection.Add(employee);
							}
							cmdReader.Close();
						}
					}
				}
				else
				{
					using (OracleDataReader reader = refCursor.GetDataReader())
					{
						while (reader.Read())
						{
							switch (dictValue)
							{
								case "ADRESY":
									{
										int id = int.Parse(ReadString(reader, 0));
										int postNum = int.Parse(ReadString(reader, 1));
										string? street = ReadString(reader, 2);
										string? city = ReadString(reader, 3);
										int postCode = int.Parse(ReadString(reader, 4));
										string? country = ReadString(reader, 5);
										Adresa address = new Adresa(id, postNum, street, city, country, postCode);
										collection.Add(address);
									}
									break;


								case "BUDOVY":
									{
										int id = int.Parse(ReadString(reader, 0));
										string? name = ReadString(reader, 1);
										int? floors = ParseNullableInt(ReadString(reader, 2));
										int addressId = int.Parse(ReadString(reader, 3));
										Budova building = new Budova(id, name, floors, addressId);
										collection.Add(building);
										break;
									}


								case "DIAGNOZY_CISELNIK":
									{
										int id = int.Parse(ReadString(reader, 0));
										string? name = ReadString(reader, 1);
										Diagnoza diagnosis = new Diagnoza(id, name);
										collection.Add(diagnosis);
										break;
									}
								case "LEKY":
									{
										int id = int.Parse(ReadString(reader, 0));
										string? name = ReadString(reader, 1);
										int price = int.Parse(ReadString(reader, 2));
										string? category = ReadString(reader, 3);
										Lek medicament = new Lek(id, name, category, price);
										collection.Add(medicament);
										break;
									}

								case "LUZKA":
									{
										int id = int.Parse(ReadString(reader, 0));
										int bedNumber = int.Parse(ReadString(reader, 1));
										int? nurseId = ParseNullableInt(ReadString(reader, 2));
										int roomId = int.Parse(ReadString(reader, 3));
										Luzko bed = new Luzko(id, bedNumber, nurseId, roomId);
										collection.Add(bed);
										break;
									}

								case "ODDELENI":
									{
										int id = int.Parse(ReadString(reader, 0));
										string? name = ReadString(reader, 1);
										int buildingId = int.Parse(ReadString(reader, 2));
										Oddeleni oddeleni = new Oddeleni(id, name, buildingId);
										collection.Add(oddeleni);
										break;
									}

								case "PACIENTI":
									{
										int id = int.Parse(ReadString(reader, 0));
										string? name = ReadString(reader, 1);
										string? surName = ReadString(reader, 2);
										DateTime birthDate = DateTime.Parse(ReadString(reader, 3));
										string? formattedBirthDate = birthDate.ToString("yyyy-MM-dd");
										string? pin = ReadString(reader, 4);
										DateTime startDate = DateTime.Parse(ReadString(reader, 5));
										string? formattedStartDate = startDate.ToString("yyyy-MM-dd");
										int doctorId = int.Parse(ReadString(reader, 6));
										int addressId = int.Parse(ReadString(reader, 7));
										int insuranceId = int.Parse(ReadString(reader, 8));
										int groupId = int.Parse(ReadString(reader, 9));
										int diagnoseId = int.Parse(ReadString(reader, 10));
										Pacient pacient = new Pacient(id, name, surName, formattedBirthDate, pin, formattedStartDate, doctorId, addressId, insuranceId, groupId, diagnoseId);
										collection.Add(pacient);
										break;
									}

								case "POJISTOVNY":
									{
										int id = int.Parse(ReadString(reader, 0));
										string? name = ReadString(reader, 1);
										int code = int.Parse(ReadString(reader, 2));
										Pojistovna pojistovna = new Pojistovna(id, name, code);
										collection.Add(pojistovna);
										break;
									}

								case "POKOJE":
									{
										int id = int.Parse(ReadString(reader, 0));
										int roomNum = int.Parse(ReadString(reader, 1));
										int hospWard = int.Parse(ReadString(reader, 2));
										Pokoj pokoj = new Pokoj(id, roomNum, hospWard);
										collection.Add(pokoj);
										break;
									}

								case "POMUCKY":
									{
										int id = int.Parse(ReadString(reader, 0));
										string? name = ReadString(reader, 1);
										int count = int.Parse(ReadString(reader, 2));
										Pomucka pomucka = new Pomucka(id, name, count);
										collection.Add(pomucka);
										break;
									}

								case "RECEPTY":
									{
										int id = int.Parse(ReadString(reader, 0));
										int docId = int.Parse(ReadString(reader, 1));
										int patId = int.Parse(ReadString(reader, 2));
										DateTime date = DateTime.Parse(ReadString(reader, 3));
										string? formattedDate = date.ToString("yyyy-MM-dd");
										Recept prescription = new Recept(id, docId, patId, formattedDate);
										collection.Add(prescription);
										break;
									}
								case "ZAMESTNANCI":
									{
										int id = int.Parse(ReadString(reader, 0));
										string? name = ReadString(reader, 1);
										string? surname = ReadString(reader, 2);
										int salary = int.Parse(ReadString(reader, 3));
										int wardId = int.Parse(ReadString(reader, 4));
										int? superiorId = ParseNullableInt(ReadString(reader, 5));
										int addressId = int.Parse(ReadString(reader, 6));
										char type = char.Parse(ReadString(reader, 7));
										Zamestnanec employee = new Zamestnanec(id, name, surname, salary, wardId, superiorId, addressId, type);
										collection.Add(employee);
										break;
									}
							}
						}
					}
				}
			}
			adminGrid.ItemsSource = collection;
		}

		public void UpdateAdminTable(object modelObject, string objectProperty, string? newValue)
		{
			if (modelObject != null)
			{
				if (newValue == "") newValue = null;
				Type objectType = modelObject.GetType();
				string objectName = objectType.Name;
				using (OracleCommand cmd = new OracleCommand())
				{
					cmd.Connection = Connection;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Clear();

					switch (objectName)
					{
						case "Adresa":
							{
								Adresa adresa = (Adresa)modelObject;
								var property = adresa.GetType().GetProperty(objectProperty);
								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(adresa, int.Parse(newValue));
											else
												property.SetValue(adresa, newValue);
										}
										else
											property.SetValue(adresa, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_adresy";
								cmd.Parameters.Add(new OracleParameter("p_id", adresa.Id));
								cmd.Parameters.Add(new OracleParameter("p_cislo_popisne", adresa.CisloPopisne));
								cmd.Parameters.Add(new OracleParameter("p_ulice", adresa.Ulice));
								cmd.Parameters.Add(new OracleParameter("p_mesto", adresa.Mesto));
								cmd.Parameters.Add(new OracleParameter("p_psc", adresa.PSC));
								cmd.Parameters.Add(new OracleParameter("p_stat", adresa.Stat));
								cmd.ExecuteNonQuery();
								break;
							}
						case "Budova":
							{
								Budova budova = (Budova)modelObject;
								var property = budova.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(budova, int.Parse(newValue));
											else
												property.SetValue(budova, newValue);
										}
										else
											property.SetValue(budova, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_budovy";
								cmd.Parameters.Add(new OracleParameter("p_id_budova", budova.Id));
								cmd.Parameters.Add(new OracleParameter("p_nazev", budova.Nazev));
								cmd.Parameters.Add(new OracleParameter("p_pocet_pater", budova.PocetPater));
								cmd.Parameters.Add(new OracleParameter("p_id_adresa", budova.IdAdresa));
								cmd.ExecuteNonQuery();
								break;
							}
						case "Diagnoza":
							{
								Diagnoza diagnoza = (Diagnoza)modelObject;
								var property = diagnoza.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(diagnoza, int.Parse(newValue));
											else
												property.SetValue(diagnoza, newValue);
										}
										else
											property.SetValue(diagnoza, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_diagnozy_ciselnik";
								cmd.Parameters.Add(new OracleParameter("p_id", diagnoza.Id));
								cmd.Parameters.Add(new OracleParameter("p_nazev", diagnoza.Nazev));
								cmd.ExecuteNonQuery();
								break;
							}

						case "Lek":
							{
								Lek lek = (Lek)modelObject;
								var property = lek.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(lek, int.Parse(newValue));
											else
												property.SetValue(lek, newValue);
										}
										else
											property.SetValue(lek, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_leky";
								cmd.Parameters.Add(new OracleParameter("p_id_lek", lek.Id));
								cmd.Parameters.Add(new OracleParameter("p_nazev", lek.Nazev));
								cmd.Parameters.Add(new OracleParameter("p_cena", lek.Cena));
								cmd.Parameters.Add(new OracleParameter("p_id_kategorie", lek.Kategorie));
								cmd.ExecuteNonQuery();
								break;
							}

						case "Pomucka":
							{
								Pomucka pomucka = (Pomucka)modelObject;
								var property = pomucka.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(pomucka, int.Parse(newValue));
											else
												property.SetValue(pomucka, newValue);
										}
										else
											property.SetValue(pomucka, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_pomucky";
								cmd.Parameters.Add(new OracleParameter("p_id", pomucka.Id));
								cmd.Parameters.Add(new OracleParameter("p_nazev", pomucka.Nazev));
								cmd.Parameters.Add(new OracleParameter("p_pocet", pomucka.Pocet));
								cmd.ExecuteNonQuery();
								break;
							}

						case "Zamestnanec":
						case "Sestra":
						case "Doktor":
							{
								Zamestnanec zamestnanec = (Zamestnanec)modelObject;
								var property = zamestnanec.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(modelObject, int.Parse(newValue));
											else
												property.SetValue(zamestnanec, newValue);
										}
										else
											property.SetValue(zamestnanec, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_zamestnanci";
								cmd.Parameters.Add(new OracleParameter("p_id_zamestnanec", zamestnanec.Id));
								cmd.Parameters.Add(new OracleParameter("p_jmeno", zamestnanec.Jmeno));
								cmd.Parameters.Add(new OracleParameter("p_prijmeni", zamestnanec.Prijmeni));
								cmd.Parameters.Add(new OracleParameter("p_plat", zamestnanec.Plat));
								cmd.Parameters.Add(new OracleParameter("p_id_oddeleni", zamestnanec.IdOddeleni));
								cmd.Parameters.Add(new OracleParameter("p_id_nadrizeny", zamestnanec.IdNadrizeneho));
								cmd.Parameters.Add(new OracleParameter("p_id_adresa", zamestnanec.IdAdresy));
								cmd.Parameters.Add(new OracleParameter("p_druh", zamestnanec.Druh));
								cmd.ExecuteNonQuery();
								break;
							}

						case "Luzko":   // jen validni id sestry z tabulky sestry, jinak se nic nestane
							{
								Luzko luzko = (Luzko)modelObject;
								var property = luzko.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(luzko, int.Parse(newValue));
											else
												property.SetValue(luzko, newValue);
										}
										else
											property.SetValue(luzko, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_luzka";
								cmd.Parameters.Add(new OracleParameter("p_id", luzko.Id));
								cmd.Parameters.Add(new OracleParameter("p_cislo", luzko.Cislo));
								cmd.Parameters.Add(new OracleParameter("p_sestra_id_zamestnanec", luzko.IdSestra));
								cmd.Parameters.Add(new OracleParameter("p_id_pokoj", luzko.IdPokoje));
								cmd.ExecuteNonQuery();
								break;
							}

						case "Pacient":
							{
								Pacient pacient = (Pacient)modelObject;
								var property = pacient.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(pacient, int.Parse(newValue));
											else
												property.SetValue(pacient, newValue);
										}
										else
											property.SetValue(pacient, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_pacienti";
								cmd.Parameters.Add(new OracleParameter("p_id_pacient", pacient.Id));
								cmd.Parameters.Add(new OracleParameter("p_jmeno", pacient.Jmeno));
								cmd.Parameters.Add(new OracleParameter("p_prijmeni", pacient.Prijmeni));
								cmd.Parameters.Add(new OracleParameter("p_datum_narozeni", DateTime.Parse(pacient.DatumNarozeni)));
								cmd.Parameters.Add(new OracleParameter("p_rodne_cislo", pacient.RodneCislo));
								cmd.Parameters.Add(new OracleParameter("p_datum_nastupu", DateTime.Parse(pacient.DatumNastupu)));
								cmd.Parameters.Add(new OracleParameter("p_id_doktor", pacient.IdDoktora));
								cmd.Parameters.Add(new OracleParameter("p_id_adresa", pacient.IdAdresy));
								cmd.Parameters.Add(new OracleParameter("p_id_pojistovna", pacient.IdPojistovny));
								cmd.Parameters.Add(new OracleParameter("p_id_skupina", pacient.IdSkupiny));
								cmd.Parameters.Add(new OracleParameter("p_id_diagnoza", pacient.IdDiagnozy));
								cmd.ExecuteNonQuery();
								break;
							}

						case "Pojistovna":
							{
								Pojistovna pojistovna = (Pojistovna)modelObject;
								var property = pojistovna.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(pojistovna, int.Parse(newValue));
											else
												property.SetValue(pojistovna, newValue);
										}
										else
											property.SetValue(pojistovna, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_pojistovny";
								cmd.Parameters.Add(new OracleParameter("p_id_pojistovna", pojistovna.Id));
								cmd.Parameters.Add(new OracleParameter("p_nazev", pojistovna.Nazev));
								cmd.Parameters.Add(new OracleParameter("p_cislo", pojistovna.Cislo));
								cmd.ExecuteNonQuery();
								break;
							}

						case "Oddeleni":
							{
								Oddeleni oddeleni = (Oddeleni)modelObject;
								var property = oddeleni.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(oddeleni, int.Parse(newValue));
											else
												property.SetValue(oddeleni, newValue);
										}
										else
											property.SetValue(oddeleni, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_oddeleni";
								cmd.Parameters.Add(new OracleParameter("p_id_oddeleni", oddeleni.ID));
								cmd.Parameters.Add(new OracleParameter("p_nazev", oddeleni.Nazev));
								cmd.Parameters.Add(new OracleParameter("p_id_budova", oddeleni.IdBudovy));
								cmd.ExecuteNonQuery();
								break;
							}

						case "Pokoj":
							{
								Pokoj pokoj = (Pokoj)modelObject;
								var property = pokoj.GetType().GetProperty(objectProperty);

								try
								{
									if (property != null)
									{
										if (newValue != null)
										{
											if (property.PropertyType == typeof(int) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(int))
												property.SetValue(pokoj, int.Parse(newValue));
											else
												property.SetValue(pokoj, newValue);
										}
										else
											property.SetValue(pokoj, null);
									}
								}
								catch (Exception ex)
								{
									showInvalidDataTypeAlert(objectName, property, newValue);
								}

								cmd.CommandText = "update_pokoje";
								cmd.Parameters.Add(new OracleParameter("p_id_pokoj", pokoj.Id));
								cmd.Parameters.Add(new OracleParameter("p_cislo_pokoje", pokoj.CisloPokoje));
								cmd.Parameters.Add(new OracleParameter("p_id_oddeleni", pokoj.IdOddeleni));
								cmd.ExecuteNonQuery();
								break;
							}
					}
				}

			}
		}

		public void DeleteAdminTable(string tableName, ref DataGrid grid, ref ComboBox comboBox)
		{
			if (grid.SelectedItem == null)
			{
				MessageBox.Show("Prosím vyberte řádek");
				return;
			}
			int id;
			switch (tableName)
			{
				case "Adresy":
					{
						Adresa data = (Adresa)grid.SelectedItem;
						id = data.Id;
						DeleteRow(tableName, id, "id_adresa");
						DeleteRow("budovy", id, "id_adresa");
						break;
					}

				case "Budovy":
					{
						Budova data = (Budova)grid.SelectedItem;
						id = data.Id;
						DeleteRow(tableName, id, "id_budova");
						break;
					}
				case "Diagnózy":
					{
						Diagnoza data = (Diagnoza)grid.SelectedItem;
						id = data.Id;
						DeleteRow("diagnozy_ciselnik", id, "id");
						break;
					}
				case "Léky":
					{
						Lek data = (Lek)grid.SelectedItem;
						id = data.Id;
						DeleteRow("leky", id, "id_lek");
						break;
					}
				case "Pomůcky":
					{
						Pomucka data = (Pomucka)grid.SelectedItem;
						id = data.Id;
						DeleteRow("pomucky", id, "id");
						break;
					}
				case "Zaměstnanci":
				case "Doktoři":
				case "Sestry":
					{
						Zamestnanec data = (Zamestnanec)grid.SelectedItem;
						id = data.Id;
						DeleteRow("zamestnanci", id, "id_zamestnanec");
						break;
					}
				case "Lůžka":
					{
						Luzko data = (Luzko)grid.SelectedItem;
						id = data.Id;
						DeleteRow("luzka", id, "id_luzko");
						break;
					}
				case "Pacienti":
					{
						Pacient data = (Pacient)grid.SelectedItem;
						id = data.Id;
						DeleteRow(tableName, id, "id_pacient");
						break;
					}
				case "Pojišťovny":
					{
						Pojistovna data = (Pojistovna)grid.SelectedItem;
						id = data.Id;
						DeleteRow("pojistovny", id, "id_pojistovna");
						break;
					}
				case "Oddělení":
					{
						Oddeleni data = (Oddeleni)grid.SelectedItem;
						id = data.ID;
						DeleteRow("oddeleni", id, "id_oddeleni");
						break;
					}
				case "Pokoje":
					{
						Pokoj data = (Pokoj)grid.SelectedItem;
						id = data.Id;
						DeleteRow(tableName, id, "id_pokoj");
						break;
					}
			}
			AdminShowAllTables(ref comboBox, ref grid);
		}

		private void DeleteRow(string tableName, int rowId, string idName)
		{
			using (OracleCommand cmd = new OracleCommand("delete_radek", Connection))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = rowId;
				cmd.Parameters.Add("p_nazev_tabulky", OracleDbType.Varchar2).Value = tableName;
				cmd.Parameters.Add("p_nazev_id", OracleDbType.Varchar2).Value = idName;

				cmd.ExecuteNonQuery();
			}
		}

		private void showInvalidDataTypeAlert(string objectName, PropertyInfo? property, string? newValue)
		{
			if (property != null)
				MessageBox.Show(objectName + " - " + property.Name + " nemůže být nastaveno na " + newValue);
		}

		public void ShowLogs()
		{
			Logs logs = new Logs();
			logs.ShowDialog();
		}

		public void ShowKatalog()
		{
			Catalog katalog = new Catalog();
			katalog.ShowDialog();
		}

		#endregion

		#region TabItem: PROFIL 
		public int SaveImageToDatabase(string filePath)
		{
			try
			{
				int newId;
				using (OracleCommand cmd = new OracleCommand())
				{
					// Uložení obrazku do ikon
					cmd.CommandText = "INSERT INTO Ikony (nazev, pripona, obsah) VALUES (:imageName, :imageType, :image) " +
						"RETURNING id_obrazek INTO :newId";
					cmd.Connection = Connection;
					cmd.Parameters.Add(new OracleParameter("imageName", GetImageNameFromFilePath(filePath)));
					cmd.Parameters.Add(new OracleParameter("imageType", GetImageTypeFromFilePath(filePath)));
					cmd.Parameters.Add(new OracleParameter("image", LoadImageBytes(filePath)));
					OracleParameter newIdParam = new OracleParameter(":newId", OracleDbType.Int32);
					newIdParam.Direction = ParameterDirection.Output;
					cmd.Parameters.Add(newIdParam);
					cmd.ExecuteNonQuery();
					newId = ((OracleDecimal)newIdParam.Value).ToInt32();

					// update vazby obrazku u uzivatele
					UpdateUserImage(newId);
				}

				MessageBox.Show("Obrázek byl úspěšně uložen do databáze.", "Info");
				return newId;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Chyba při ukládání obrázku: " + ex.Message, "Chyba");
			}
			return 0;
		}

		private int GetUzivatelskeIdObrazku()
		{
			if (Uzivatel != null)
			{
				using (OracleCommand cmd = new OracleCommand("SELECT id_obrazek FROM uzivatele_s_ikony WHERE uzivatel_nazev LIKE :userName", Connection))
				{
					// Vytahnout id obrazku pro update z uzivatele_s_ikony view
					cmd.Parameters.Add(new OracleParameter("userName", Uzivatel.Jmeno));
					object obj = cmd.ExecuteScalar();
					if (obj != null)
						if (int.TryParse(obj.ToString(), out int imageId))
							return imageId;
				}
			}

			return 0;
		}

		public int UpdateImageInDatabase(string filePath)
		{
			using (OracleCommand cmd = new OracleCommand("update_ikony", Connection))
			{
				int userImgId = GetUzivatelskeIdObrazku();
				if (userImgId != 0)
				{
					// update obsahu daneho id v tabulce ikony
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = userImgId;
					cmd.Parameters.Add("p_nazev", OracleDbType.Varchar2).Value = GetImageNameFromFilePath(filePath);
					cmd.Parameters.Add("p_obsah", OracleDbType.Blob).Value = LoadImageBytes(filePath);
					cmd.Parameters.Add("p_pripona", OracleDbType.Varchar2).Value = GetImageTypeFromFilePath(filePath);
					cmd.ExecuteNonQuery();

					// update vazby obrazku u uzivatele
					UpdateUserImage(userImgId);
					return userImgId;
				}
				else
				{
					MessageBox.Show("Chyba při update obrazku v databazi", "Error");
				}
				return 0;
			}
		}

		public BitmapImage? LoadImageContentFromDatabase(int? idImage)
		{
			if (idImage == null)
				return null;
			try
			{
				using (OracleCommand cmd = new OracleCommand("SELECT obsah FROM Ikony WHERE id_obrazek = :idImage", Connection))
				{
					cmd.Parameters.Add(new OracleParameter("idImage", idImage));
					OracleDataReader reader = cmd.ExecuteReader();

					if (reader.Read())
					{
						// Načítání bytů z databáze
						byte[] imageBytes = (byte[])reader["obsah"];

						// Vytvoření BitmapImage z načtených bytů
						BitmapImage bitmap = new BitmapImage();
						bitmap.BeginInit();
						bitmap.CacheOption = BitmapCacheOption.OnLoad;
						bitmap.StreamSource = new MemoryStream(imageBytes);
						bitmap.EndInit();
						return bitmap;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Chyba při načítání obrázku: " + ex.Message, "Chyba");
			}
			return null;
		}

		private byte[] LoadImageBytes(string filePath)
		{
			byte[] imageBytes = File.ReadAllBytes(filePath);
			return imageBytes;
		}

		private string? GetImageTypeFromFilePath(string filePath)
		{
			string[] parts = filePath.Split('\\', '/');
			string fileName = parts[parts.Length - 1];
			fileName = fileName.Substring(fileName.IndexOf("."));
			return fileName;
		}

		private string? GetImageNameFromFilePath(string filePath)
		{
			string[] parts = filePath.Split('\\', '/');
			string fileName = parts[parts.Length - 1];
			fileName = fileName.Substring(0, fileName.IndexOf("."));
			return fileName;
		}

		private void UpdateUserImage(int imgId)
		{
			using (OracleCommand cmd = new OracleCommand("update_uzivatelsky_obrazek", Connection))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new OracleParameter("p_nazev", Uzivatel.Jmeno));
				cmd.Parameters.Add(new OracleParameter("p_id_obrazku", imgId));
				cmd.ExecuteNonQuery();
			}
		}

		public bool DeleteCurrentUserImageFromDatabase()
		{
			int userImgId = GetUzivatelskeIdObrazku();
			if (userImgId != 0)
			{
				using (OracleCommand cmd = new OracleCommand("UPDATE uzivatele SET id_obrazku = NULL WHERE id_obrazku = :imgId", Connection))
				{
					// smazání vazby v tabulce uzivatele
					cmd.Parameters.Add(new OracleParameter("imgId", userImgId));
					cmd.ExecuteNonQuery();

					// smazání daného obrázku z databáze
					cmd.Parameters.Clear();
					cmd.CommandText = "DELETE FROM Ikony WHERE id_obrazek = :imgId";
					cmd.Parameters.Add(new OracleParameter("imgId", userImgId));
					int rowsAffected = cmd.ExecuteNonQuery();
					if (rowsAffected > 0)
					{
						MessageBox.Show("Obrázek byl úspěšně smazán", "Info");
						return true;
					}
					else
					{
						MessageBox.Show("Obrázek se nepovedlo smazat", "Warn");
						return false;
					}

				}
			}
			return false;
		}

		public BitmapImage? LoadLoggedUser(bool guest)
		{
			if (!guest && Uzivatel != null)
			{
				BitmapImage? bitMapImage = null;
				string query = "SELECT id_obrazek FROM uzivatele_s_ikony WHERE uzivatel_nazev LIKE :userName";
				OracleCommand cmd = new OracleCommand(query, Connection);
				cmd.Parameters.Add(new OracleParameter("userName", Uzivatel.Jmeno));
				object imgIdObject = cmd.ExecuteScalar();
				if (imgIdObject != null)    // našel se odpovidajici zaznam v pohledu uzivatele_s_ikony
				{
					int imgId = Decimal.ToInt32((decimal)imgIdObject);
					bitMapImage = LoadImageContentFromDatabase(imgId);
					return bitMapImage;
				}
			}
			return null;
		}
		#endregion

		#region TabItem: PACIENTI
		public void PacientsComboBoxesHandle(ref ComboBox skupiny, ref ComboBox diagnozy)
		{
			// Nastavení příkazů pro funkce
			string skupinyCmd = "BEGIN :result := GetKrevniSkupiny; END;";
			string diagnozyCmd = "BEGIN :result := GetDiagnozy; END;";

			// Vytvoření transakce
			using (OracleTransaction transaction = Connection.BeginTransaction())
			{
				try
				{
					// Volání první funkce
					using (OracleCommand command1 = new OracleCommand(skupinyCmd, Connection))
					{
						command1.CommandType = CommandType.Text;
						command1.Parameters.Add("result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
						command1.Transaction = transaction;

						using (OracleDataReader reader1 = command1.ExecuteReader())
						{
							skupiny.Items.Clear();

							while (reader1.Read())
							{
								string typ = ReadString(reader1, "typ");
								string id = ReadString(reader1, "id_skupina");
								KrevniSkupina ks = new KrevniSkupina(int.Parse(id), typ);
								KrevniSkupiny.Add(ks);
								skupiny.Items.Add(ks);
							}
						}
					}

					// Volání druhé funkce
					using (OracleCommand command2 = new OracleCommand(diagnozyCmd, Connection))
					{
						command2.CommandType = CommandType.Text;
						command2.Parameters.Add("result", OracleDbType.RefCursor, System.Data.ParameterDirection.ReturnValue);
						command2.Transaction = transaction;

						using (OracleDataReader reader2 = command2.ExecuteReader())
						{
							// Vyčištění ComboBoxu před přidáním nových dat
							diagnozy.Items.Clear();

							// Zpracování výsledků druhé funkce a naplnění ComboBoxu
							while (reader2.Read())
							{
								string nazev = ReadString(reader2, "nazev");
								string id = ReadString(reader2, "id");
								Diagnoza d = new Diagnoza(int.Parse(id), nazev);
								diagnozy.Items.Add(d);
								Diagnozy.Add(d);
							}
						}
					}

					// Commit transakce
					transaction.Commit();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Chyba: " + ex.Message);

					transaction.Rollback();
				}
			}
			skupiny.SelectedIndex = 0;
			diagnozy.SelectedIndex = 0;

		}

		public void ShowPacients(ref DataGrid pacientsGrid, ref ComboBox comboBox, Ciselnik druhCiselniku)
		{
			pacientsGrid.IsReadOnly = true;
			pacientsGrid.Columns.Clear();

			switch (druhCiselniku)
			{
				case Ciselnik.DIAGNOZY:
					{
						using (OracleCommand command = new OracleCommand("BEGIN :result := GetPacientiByDiagnoza(:id_param); END;", Connection))
						{
							command.CommandType = CommandType.Text;
							Diagnoza? selectedDiagnoza = comboBox.SelectedValue as Diagnoza;
							if (selectedDiagnoza != null)
							{
								int id = selectedDiagnoza.Id;

								command.Parameters.Add("result", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
								command.Parameters.Add("id_param", OracleDbType.Int32).Value = id;
								command.ExecuteNonQuery();

								using (OracleDataReader reader = ((OracleRefCursor)command.Parameters["result"].Value).GetDataReader())
								{
									DataTable dataTable = new DataTable();
									dataTable.Load(reader);

									pacientsGrid.ItemsSource = dataTable.DefaultView;
								}
							}
						}
						break;
					}
				case Ciselnik.SKUPINY:
					{
						using (OracleCommand command = new OracleCommand("BEGIN :result := GetPacientiBySkupina(:id_param); END;", Connection))
						{
							command.CommandType = CommandType.Text;
							KrevniSkupina? selectedSkupina = comboBox.SelectedValue as KrevniSkupina;
							if (selectedSkupina != null)
							{
								int id = selectedSkupina.Id;
								command.Parameters.Add("result", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
								command.Parameters.Add("id_param", OracleDbType.Int32).Value = id;
								command.ExecuteNonQuery();

								using (OracleDataReader reader = ((OracleRefCursor)command.Parameters["result"].Value).GetDataReader())
								{
									DataTable dataTable = new DataTable();
									dataTable.Load(reader);

									pacientsGrid.ItemsSource = dataTable.DefaultView;
								}
							}
						}
						break;
					}
				case Ciselnik.NONE:
					{
						using (OracleCommand command = new OracleCommand("BEGIN :result := GetPacienti(); END;", Connection))
						{
							command.CommandType = CommandType.Text;

							command.Parameters.Add("result", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
							command.ExecuteNonQuery();

							using (OracleDataReader reader = ((OracleRefCursor)command.Parameters["result"].Value).GetDataReader())
							{
								DataTable dataTable = new DataTable();
								dataTable.Load(reader);

								pacientsGrid.ItemsSource = dataTable.DefaultView;
							}
						}
					}
					break;
			}
			pacientsGrid.Columns[0].Visibility = Visibility.Hidden;

			if (Uzivatel != null && Uzivatel.Role != Role.PRIMAR)
			{
				pacientsGrid.Columns[3].Visibility = Visibility.Hidden;
				pacientsGrid.Columns[4].Visibility = Visibility.Hidden;
			}
		}

		public void AddPacient(ref DataGrid gridView)
		{
			PacientDialog dialog = new PacientDialog(Uzivatel);
			dialog.ShowDialog();
		}
		public void EditPacient(ref DataGrid gridView)
		{
			try
			{
				DataGridRow row = (DataGridRow)gridView.ItemContainerGenerator.ContainerFromItem(gridView.SelectedItem);
				var cells = row.Item as DataRowView;

				string[] values = cells.Row.ItemArray.Select(cell => cell.ToString()).ToArray();

				string prvniSloupec = values[0];
				PacientDialog dialog = new PacientDialog(int.Parse(prvniSloupec), Uzivatel);
				dialog.ShowDialog();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Není vybrán žádný pacient.");
			}
		}


		public void FindPacient(ref DataGrid pacientiGrid, ref TextBox pacientsPacientTb)
		{
			string? pacient = pacientsPacientTb.Text;
			if (pacient != null)
			{
				using (OracleCommand command = new OracleCommand("BEGIN :result := NajdiPacientaPodleJmena(:pacient); END;", Connection))
				{
					command.CommandType = CommandType.Text;
					command.Parameters.Add("result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
					command.Parameters.Add(new OracleParameter("p_pacient", pacient));
					command.ExecuteNonQuery();

					using (OracleDataReader reader = ((OracleRefCursor)command.Parameters["result"].Value).GetDataReader())
					{
						DataTable dataTable = new DataTable();
						dataTable.Load(reader);

						pacientiGrid.ItemsSource = dataTable.DefaultView;
					}
				}
			}
			pacientiGrid.Columns[0].Visibility = Visibility.Hidden;

			if (Uzivatel != null && Uzivatel.Role != Role.PRIMAR)
			{
				pacientiGrid.Columns[3].Visibility = Visibility.Hidden;
				pacientiGrid.Columns[4].Visibility = Visibility.Hidden;
			}
		}

		#endregion

		#region TabItem: RECEPTY
		public void RecipeesComboBoxHandle(ref ComboBox recipeesComboBox)
		{
			recipeesComboBox.Items.Add("Vše");
			using (OracleCommand command = new OracleCommand("SELECT nazev_kategorie FROM kategorie_leku_ciselnik", Connection))
			{
				using (OracleDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						// Přidání hodnot do ComboBox
						string? nazevKategorie = ReadString(reader, "nazev_kategorie");
						recipeesComboBox.Items.Add(nazevKategorie);
					}
					recipeesComboBox.SelectedIndex = 0;
				}
			}
		}

		public void ShowRecipees(ref ComboBox recipeesComboBox, ref DataGrid recipeesGrid)
		{
			string? category = recipeesComboBox.SelectedValue as string;
			if (category == "Vše") category = "all";
			using (OracleCommand command = new OracleCommand("BEGIN :result := GetReceptyLekyByKategorie(:category); END;", Connection))
			{
				command.CommandType = CommandType.Text;

				command.Parameters.Add("result", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
				command.Parameters.Add(new OracleParameter("category", category));
				command.ExecuteNonQuery();

				using (OracleDataReader reader = ((OracleRefCursor)command.Parameters["result"].Value).GetDataReader())
				{
					DataTable dataTable = new DataTable();
					dataTable.Load(reader);

					recipeesGrid.ItemsSource = dataTable.DefaultView;
				}
			}
		}
		#endregion

		#region TabItem: Uzivatele

		public List<Uzivatel> LoadAllUsers()
		{
			List<Uzivatel> users = new List<Uzivatel>();
			using (OracleCommand command = new OracleCommand("SELECT nazev, role FROM uzivatele", Connection))
			{
				using (OracleDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						// Přidání hodnot do ComboBox
						string? username = ReadString(reader, "nazev");
						string roleString = ReadString(reader, "role");
						if (Enum.TryParse<Role>(roleString, out Role role))
						{
							Uzivatel user = new Uzivatel(username, role);
							users.Add(user);
						}
					}
				}
			}
			return users;
		}

		public void UpdateUserFromAdminTab(string originalUserName, string newUserName, string roleString)
		{
			using (OracleCommand command = new OracleCommand("update_uzivatele_admin", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add("p_stary_nazev", originalUserName);
				command.Parameters.Add("p_novy_nazev", newUserName);
				command.Parameters.Add("p_role", roleString);
				command.ExecuteNonQuery();
			}
		}

		public void DeleteUserFromAdminTab(string userName)
		{
			using (OracleCommand command = new OracleCommand("delete_uzivatele_admin", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add("p_nazev", userName);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region TabItem: Rozpis

		public void ScheduleComboBoxHandle(ref ComboBox scheduleNurseCb)
		{
			scheduleNurseCb.Items.Add("Vše");
			using (OracleCommand command = new OracleCommand("SELECT prijmeni FROM zamestnanci z JOIN sestry s ON z.id_zamestnanec = s.id_zamestnanec", Connection))
			{
				using (OracleDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						// Přidání hodnot do ComboBox
						string? prijmeni = ReadString(reader, "prijmeni");
						scheduleNurseCb.Items.Add(prijmeni);
					}
					scheduleNurseCb.SelectedIndex = 0;
				}
			}
		}

		public void ShowShedule(ref ComboBox scheduleNurseCb, ref DataGrid scheduleGrid)
		{
			string? surname = scheduleNurseCb.SelectedValue as string;
			if (surname == "Vše") surname = "all";
			using (OracleCommand command = new OracleCommand("BEGIN :result := GetRozpisBySestra(:surname); END;", Connection))
			{
				command.CommandType = CommandType.Text;

				command.Parameters.Add("result", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
				command.Parameters.Add(new OracleParameter("p_prijmeni_sestry", surname));
				command.ExecuteNonQuery();

				using (OracleDataReader reader = ((OracleRefCursor)command.Parameters["result"].Value).GetDataReader())
				{
					DataTable dataTable = new DataTable();
					dataTable.Load(reader);

					scheduleGrid.ItemsSource = dataTable.DefaultView;
				}

			}
		}

		#endregion

		#region TabItem: Zamestnanci
		public void ShowEmployees(ref DataGrid employeesGrid)
		{
			using (OracleCommand command = new OracleCommand("BEGIN :result := GetZamestnanciSNadrizenymi(); END;", Connection))
			{
				command.CommandType = CommandType.Text;
				command.Parameters.Add("result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
				command.ExecuteNonQuery();

				using (OracleDataReader reader = ((OracleRefCursor)command.Parameters["result"].Value).GetDataReader())
				{
					DataTable dataTable = new DataTable();
					dataTable.Load(reader);

					employeesGrid.ItemsSource = dataTable.DefaultView;
				}
			}
			employeesGrid.Columns[0].Visibility = Visibility.Hidden;
		}

		#endregion

		#region TabItem: Zadosti
		public void DenyRequest(ref DataGrid requestsGrid, int id)
		{
			using (OracleCommand command = new OracleCommand("DELETE_NESCHVALENE_ZMENY", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new OracleParameter("p_id", id));
				command.ExecuteNonQuery();
				ShowRequests(ref requestsGrid);
			}
		}

		public void ShowRequests(ref DataGrid requestsGrid)
		{
			using (OracleCommand command = new OracleCommand("BEGIN :result := GetNeschvaleneZadosti(); END;", Connection))
			{
				command.CommandType = CommandType.Text;
				command.Parameters.Add("result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
				command.ExecuteNonQuery();

				using (OracleDataReader reader = ((OracleRefCursor)command.Parameters["result"].Value).GetDataReader())
				{
					DataTable dataTable = new DataTable();
					dataTable.Load(reader);

					requestsGrid.ItemsSource = dataTable.DefaultView;
				}
				requestsGrid.Columns[0].Visibility = Visibility.Hidden;
			}
		}

		public bool AcceptRequest(ref DataGrid requestsGrid, int id)
		{
			using (OracleCommand command = new OracleCommand("ZpracujZadostPacienti", Connection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new OracleParameter("p_id", id));
				int results = command.ExecuteNonQuery();
				ShowRequests(ref requestsGrid);
				if (results == 0)
				{
					return false;
				}
				return true;
			}
		}

		#endregion

		#region ReaderMetody
		private string ReadString(OracleDataReader reader, int columnIndex)
		{
			return reader.IsDBNull(columnIndex) ? "..." : reader.GetString(columnIndex);
		}

		private string ReadString(OracleDataReader reader, string columnName)
		{
			int columnIndex = reader.GetOrdinal(columnName);
			return ReadString(reader, columnIndex);
		}

		private int? ParseNullableInt(string input)
		{
			if (int.TryParse(input, out int result))
			{
				return result;
			}
			else
			{
				return null;
			}
		}

		#endregion
	}
}