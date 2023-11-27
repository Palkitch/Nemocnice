using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nemocnice.Model;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;

namespace Nemocnice.Database
{
    public class DatabaseConnection
    {
        private readonly string? ConnectionString;

        private static DatabaseConnection? databaseConnection;
        public OracleConnection OracleConnection { get; }
        private string jsonPath;

        public static DatabaseConnection Instance
        {
            get
            {
                if (databaseConnection == null)
                {
                    databaseConnection = new DatabaseConnection();
                }
                return databaseConnection;
            }
        }

        private DatabaseConnection()
        {
            jsonPath = "Connection.json";
            ConnectionString = GetConnectionStringFromJson(jsonPath);
            if (ConnectionString != null)
            {
                OracleConnection = new OracleConnection(ConnectionString);
            }
            else 
            {
                MessageBox.Show("Nepovedlo se přečíst soubor " + jsonPath, "Chyba");
                Environment.Exit(0);    
            }
        }

        private string? GetConnectionStringFromJson(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    DatabaseConfiguration config = JsonConvert.DeserializeObject<DatabaseConfiguration>(json);
                    if (config != null) 
                    {
                        string connString = config.ConnectionString;
                        if (connString != null)
                        {
                            return connString;
                        }
                    }
                    return null;

                }
                else
                {
                    Console.WriteLine("JSON soubor neexistuje.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chyba při čtení souboru: " + ex.Message);
                return null;
            }
        }
    }
}
