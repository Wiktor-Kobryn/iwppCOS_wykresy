using LiveCharts;
using LiveChartsCore;
using Microsoft.Win32;
using System.Windows;


namespace IwppCOSwykresy;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();


        for (int i = 0; i < seriesCountMax; i++)
            isSeriesChosen.Add(true);
    }

    private string textLabelDeltaTime = "Odstęp czasowy pomiarów: ";

    private string sourceFileNameCSV = "";
    private RawData rawData;

    private int dataToViewStart = 0;
    public List<SeriesCollection> SeriesCollectionRawData { get; set; } = new List<SeriesCollection>();
    private List<bool> isSeriesChosen = new List<bool>();
    private double pointSize = 5;
    private int seriesCountMax = 6;

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
            UpdateRawChart();
        }
    }

    private void UpdateRawChart()
    {
        var allSeries = new List<ISeries>();

        for (int i = 0; i < seriesCountMax; i++)
        {
            if (isSeriesChosen[i])
                allSeries.AddRange(rawData.GenerateSeries(i, pointSize));
        }

        chartRawData.Series = allSeries;
    }
}