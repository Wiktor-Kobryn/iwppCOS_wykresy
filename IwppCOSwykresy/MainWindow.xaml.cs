using LiveCharts;
using LiveChartsCore;
using Microsoft.Win32;
using SkiaSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace IwppCOSwykresy;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string textLabelDeltaTime = "Odstęp czasowy pomiarów: ";

    private string sourceFileNameCSV = "";
    private RawData rawData;

    private int dataToViewStart = 0;
    private List<SKColor> seriesColors;
    private List<CheckBox> allCheckboxes;
    private List<bool> isSeriesChosen = new List<bool>();
    private double pointSize = 5;
    private int seriesCountMax = 6;
    private bool isAfterInit = false, isDataLoaded = false;

    public MainWindow()
    {
        InitializeComponent();


        for (int i = 0; i < seriesCountMax; i++)
            isSeriesChosen.Add(true);

        allCheckboxes = new List<CheckBox> { cbxDane0, cbxDane1, cbxDane2, cbxDane3, cbxDane4, cbxDane5 };
        seriesColors = new List<SKColor> { SKColors.IndianRed, SKColors.CornflowerBlue, SKColors.Orange, SKColors.GreenYellow, SKColors.MediumPurple, SKColors.Yellow };

        isAfterInit = true;
    }

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

            rawData = new RawData(seriesCountMax);
            rawData.ReadFromAFile(sourceFileNameCSV);

            lblDeltaTime.Content = textLabelDeltaTime + rawData.DeltaTime + "s";
            isDataLoaded = true;
            UpdateRawChart();
        }
    }

    private void UpdateRawChart()
    {
        var allSeries = new List<ISeries>();

        for (int i = 0; i < seriesCountMax; i++)
        {
            if (isSeriesChosen[i])
                allSeries.AddRange(rawData.GenerateSeries(i, pointSize, dataToViewStart, seriesColors[i]));
        }

        chartRawData.Series = allSeries;
    }

    private void cbxDane_Click(object sender, RoutedEventArgs e)
    {
        if(isDataLoaded)
        {
            for (int i = 0; i < allCheckboxes.Count; i++)
            {
                isSeriesChosen[i] = allCheckboxes[i].IsChecked ?? false;
            }

            int checkedCount = isSeriesChosen.Count(x => x);

            if (checkedCount <= 2)
            {
                for (int i = 0; i < allCheckboxes.Count; i++)
                {
                    if (isSeriesChosen[i])
                        allCheckboxes[i].IsEnabled = false;
                }
            }
            else
            {
                foreach (var cb in allCheckboxes)
                    cb.IsEnabled = true;
            }

            UpdateRawChart();
        }
        else
        {
            foreach (var cb in allCheckboxes)
                cb.IsEnabled = true;
        }

    }

    private void txtStartValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (isDataLoaded == false)
            tbcMainTab.SelectedIndex = 0;
            
    }

    private void txtStartValue_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isAfterInit && isDataLoaded && int.TryParse(txtStartValue.Text, out int value))
        {
            if (value < 0)
                txtStartValue.Text = "0";
            else if (value > 99)
                txtStartValue.Text = "99";

            dataToViewStart = value;
            lblStartTime.Content = "( " + (value * rawData.DeltaTime) + "s )";
            UpdateRawChart();
        }
    }

}