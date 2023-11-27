using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public enum Role
    {
        [Description("primar")]
        PRIMAR, // admin
        [Description("doktor")]
        DOKTOR, // user
        [Description("sestra")]
        SESTRA  // user
    }

    public class Uzivatel
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
