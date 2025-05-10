using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Win32;
using SkiaSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace IwppCOSwykresy;


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

        allCheckboxes = new List<CheckBox> { cbxData0, cbxData1, cbxData2, cbxData3, cbxData4, cbxData5 };
        seriesColors = new List<SKColor> { SKColors.IndianRed, SKColors.CornflowerBlue, SKColors.Orange, SKColors.GreenYellow, SKColors.MediumPurple, SKColors.Yellow };
        UpdateDataLabelColors();

        isAfterInit = true;

        Func<double, string> xLabeler = value => $"{value:0.0}";
        Func<double, string> yLabeler = value => $"{value:0.00}";

        chartRawData.XAxes =
        [
            new Axis
            {
                Name = "Czas t [s]",
                Labeler = xLabeler,
                MinStep = rawData != null ? rawData.DeltaTime : 1,       
                SeparatorsPaint = new SolidColorPaint(new SKColor(200,200,200)),
            }
        ];

        chartRawData.YAxes =
        [
            new Axis
            {
                Name = "Stężenie",
                Labeler = yLabeler,
                SeparatorsPaint = new SolidColorPaint(new SKColor(200,200,200)),
            }
        ];
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

    private void cbxData_Click(object sender, RoutedEventArgs e)
    {
        if (isDataLoaded)
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

    private SolidColorBrush ConvertSkColorToBrush(SKColor skColor)
    {
        return new SolidColorBrush(Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue));
    }


    private void UpdateDataLabelColors()
    {
        rectColor0.Fill = ConvertSkColorToBrush(seriesColors[0]);
        rectColor1.Fill = ConvertSkColorToBrush(seriesColors[1]);
        rectColor2.Fill = ConvertSkColorToBrush(seriesColors[2]);
        rectColor3.Fill = ConvertSkColorToBrush(seriesColors[3]);
        rectColor4.Fill = ConvertSkColorToBrush(seriesColors[4]);
        rectColor5.Fill = ConvertSkColorToBrush(seriesColors[5]);
    }
}