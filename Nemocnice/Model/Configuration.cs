using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.Model
{
    public class DatabaseConfiguration
    {
        public string ConnectionString { get; set; }

        private DatabaseConfiguration() 
        {
            ConnectionString = string.Empty;
        }
    }
}
