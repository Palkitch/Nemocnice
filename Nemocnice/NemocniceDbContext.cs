using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Nemocnice
{
    public class NemocniceDbContext : DbContext
    {
        public NemocniceDbContext() : base("User Id=st67082;Password=abcde;" +
            "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=fei-sql3.upceucebny.cz)" +
            "(PORT=1521))(CONNECT_DATA=(SID=BDAS)(SERVER=DEDICATED)))")
        { 
        
        }

    }
}
