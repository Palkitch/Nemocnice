using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Controls;
using System.Windows.Data;
using Nemocnice.Database;

namespace Nemocnice
{
    public partial class MainWindow : Window
    {
        private DatabaseHandler handler;

        public MainWindow()
        {
            InitializeComponent();
            Login login = new Login();
            login.ShowDialog();
            handler = new DatabaseHandler();
            handler.ComboBoxHandle(ref comboBox);
        }

        private void printButtonOnAction(object sender, RoutedEventArgs e)
        {
            handler.switchMethod(ref resultLabel, ref comboBox, ref grid);
        }

        private void menuItemShowClick(object sender, RoutedEventArgs e)
        {
            //TODO: show uvodni okno
        }

        private void menuItemAddOrRemoveClick(object sender, RoutedEventArgs e)
        {
            //TODO: show add/edit dialog kde se budou měnit textfields s parametry pro přidání dle tabulky
        }

        private void menuItemEditClick(object sender, RoutedEventArgs e)
        {
            //TODO: vzit selected index a ten upravit v dialogu
        }
    }

}
