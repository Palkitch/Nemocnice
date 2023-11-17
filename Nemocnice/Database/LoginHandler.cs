using Nemocnice.ModelObjects;
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
        private readonly DatabaseHandler databaseHandler;

        public LoginHandler()
        {
            databaseHandler = DatabaseHandler.Instance;
        }

        public bool Register(string username, string password)
        {
            return databaseHandler.Register(username, HashPassword(password));
        }

        public bool Login(string username, string password)
        {
            password = HashPassword(password);
            string? storedHashedPassword = databaseHandler.Login(username, password);

            if (storedHashedPassword != null && storedHashedPassword == password) {
                return true;
            }
            else
            {
                MessageBox.Show("Nesprávné heslo", "Upozornění");
                return false;
            }
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
    }
}