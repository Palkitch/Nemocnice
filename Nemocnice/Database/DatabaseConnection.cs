using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Nemocnice.Database
{
    public class DatabaseConnection
    {
        private readonly string ConnectionString;

        private static DatabaseConnection? databaseConnection;
        public OracleConnection OracleConnection { get; }

        private DatabaseConnection()
        {
            // db connection creation with connection string
            ConnectionString = "User Id=st67082;Password=abcde;" +
                "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=fei-sql3.upceucebny.cz)" +
                "(PORT=1521))(CONNECT_DATA=(SID=BDAS)(SERVER=DEDICATED)))";
            OracleConnection = new OracleConnection(ConnectionString);
        }

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

    }
}
