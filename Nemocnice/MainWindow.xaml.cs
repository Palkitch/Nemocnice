using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using Nemocnice.Config;

namespace Nemocnice
{
    public partial class MainWindow : Window
    {
        Launcher Launcher { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Launcher = new Launcher(this);
            Launcher.Launch();
        }

        private void ProfInsertPicture_Click(object sender, RoutedEventArgs e)
        {
            Launcher.ProfInsertPicture_Click();
        }

        private void ProfDeletePicture_Click(object sender, RoutedEventArgs e)
        {
            Launcher.ProfDeletePicture_Click();
        }

        private void AdminShowTables_Click(object sender, RoutedEventArgs e)
        {
            Launcher.AdminShowTables_Click();
        }

        private void AdminShowLogs_Click(object sender, RoutedEventArgs e)
        {
            Launcher.AdminShowLogs_Click();
        }

        private void PacientsShowTable_Click(object sender, RoutedEventArgs e)
        {
            Launcher.PacientsShowTable_Click();
        }

        private void RecipeesShowTable_Click(object sender, RoutedEventArgs e)
        {
            Launcher.RecipeesShowTable_Click();
        }

        private void AdminShowKatalog_Click(object sender, RoutedEventArgs e)
        {
            Launcher.AdminShowKatalog_Click();
        }

        private void PacientsAdd_Click(object sender, RoutedEventArgs e)
        {
            Launcher.AddPacient_Click();
        }
    }
}
