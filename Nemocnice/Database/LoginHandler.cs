using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Nemocnice.Database
{
    public class LoginHandler
    {
        OracleConnection oracleConnection;
        DatabaseConnection dbConnection;
        public LoginHandler()
        {
            dbConnection = DatabaseConnection.Instance;
            oracleConnection = dbConnection.OracleConnection;
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public void Register(string username, string password)
        {
            if (UserExists(username, oracleConnection))
            {
                MessageBox.Show($"Uživatel s jménem {username} již existuje.", "Upozornění");
                return;
            }
            string insertQuery = "INSERT INTO Uzivatele (username, password) VALUES (:uname, :pwd)";
            OracleCommand cmd = new OracleCommand(insertQuery, oracleConnection);
            cmd.Parameters.Add(new OracleParameter("uname", username));
            cmd.Parameters.Add(new OracleParameter("pwd", HashPassword(password)));
            cmd.ExecuteNonQuery();
        }
        public void Login(string username, string password)
        {
            if (!UserExists(username, oracleConnection))
            {
                MessageBox.Show($"Uživatel s jménem {username} neexistuje.", "Upozornění");
                return;
            }

            string storedHashedPassword = GetHashedPassword(username, oracleConnection);
            string inputHashedPassword = HashPassword(password);

            if (storedHashedPassword == inputHashedPassword)
            {
                MessageBox.Show("Přihlášení úspěšné", "Přihlášení");
            }
            else
            {
                MessageBox.Show("Nesprávné heslo", "Upozornění");
            }
        }
        private bool UserExists(string username, OracleConnection connection)
        {
            string query = "SELECT COUNT(*) FROM Uzivatele WHERE username = :uname";
            OracleCommand cmd = new OracleCommand(query, connection);
            cmd.Parameters.Add(new OracleParameter("uname", username));
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
        private string GetHashedPassword(string username, OracleConnection connection)
        {
            string query = "SELECT password FROM Uzivatele WHERE username = :uname";
            OracleCommand cmd = new OracleCommand(query, connection);
            cmd.Parameters.Add(new OracleParameter("uname", username));
            return cmd.ExecuteScalar().ToString();
        }
    }
}