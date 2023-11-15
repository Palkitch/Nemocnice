using Nemocnice.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nemocnice
{

    public partial class Login : Window
    {

        LoginHandler loginHandler;

        public Login()
        {
            InitializeComponent();
            loginHandler = new LoginHandler();
        }



        private void btnLoginClick(object sender, RoutedEventArgs e)
        {
            string password = new NetworkCredential(string.Empty, pbPassword.SecurePassword).Password;
            string username = tbLogin.Text;
            if (username.Length > 0 && password.Length > 0)
            {
                bool loginResult = loginHandler.Login(username, password);

                if (loginResult)
                {
                    // Nastavte DialogResult na true a uzavřete okno
                    DialogResult = true;
                }
                else
                {
                    // Zde můžete obsloužit případ neúspěšného přihlášení
                    MessageBox.Show("Neplatné přihlašovací údaje.");
                }
            }
        }

        private void btnRegisterClick(object sender, RoutedEventArgs e)
        {
            string password = new NetworkCredential(string.Empty, pbPassword.SecurePassword).Password;
            string username = tbLogin.Text;
            if (username.Length > 0 && password.Length > 0)
            {
                bool registerResult = loginHandler.Register(username, password);

                if (registerResult)
                {
                    // Nastavte DialogResult na true a uzavřete okno
                    DialogResult = true;
                }
                
            }
        }
    }
}
