using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public enum Role
    {
        Admin,
        User
    }

    internal class Uzivatel
    {
        public string Jmeno { get; set; }
        public Role Role { get; set; }
        public Uzivatel(string jmeno, Role role)
        {
            this.Jmeno = jmeno;
            this.Role = role;
        }
    }
}
