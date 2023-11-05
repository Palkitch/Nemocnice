using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.Database
{
    public class LoginHandler
    {
        OracleConnection oracleConnection;

        public LoginHandler() 
        {
            DatabaseConnection dbConnection = DatabaseConnection.Instance;
            oracleConnection = dbConnection.OracleConnection;
        }

        // TODO: vyřešit login a registraci
        // TODO: implementovat zde nějaký hash

        public void register(string username, string password) 
        {
            Console.WriteLine(username + ":" + password); 
        }

        public void login(string username, string password)
        {
            Console.WriteLine(username + ":" + password);
        }
    }
}
