﻿using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IwppCOSwykresy;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private string sourceFileNameCSV = "";
    private RawData rawData = new RawData();

    private void btnLoadFile_Click(object sender, RoutedEventArgs e)
    {
        LoadFileCSV();
    }

    private void LoadFileCSV()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Pliki CSV (*.csv)|*.csv";

        if (openFileDialog.ShowDialog() == true)
        {
            sourceFileNameCSV = openFileDialog.FileName;

            rawData.ReadFromAFile(sourceFileNameCSV);
        }
    }
}