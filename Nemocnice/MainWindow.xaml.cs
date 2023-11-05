using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using Nemocnice.ModelObjects;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Controls;
using System.Windows.Data;

namespace Nemocnice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //OracleConnection connection;
        private DatabaseHandler handler;

        public MainWindow()
        {
            InitializeComponent();
            handler = new DatabaseHandler();
            handler.ComboBoxHandle(ref comboBox);
        }

        private void printButtonOnAction(object sender, RoutedEventArgs e)
        {
            handler.switchMethod(ref resultLabel, ref comboBox, ref grid);
        }
    }

}
