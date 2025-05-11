using LiveCharts.Defaults;
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
    private List<CheckBox> rawCheckboxes;
    private List<CheckBox> normCheckboxes;
    private List<bool> isSeriesChosen = new List<bool>();
    private double pointSize = 5;
    private int seriesCountMax = 6;
    private bool isAfterInit = false, isDataLoaded = false;
    private double? zoneLow = null;
    private double? zoneHigh = null;
    public MainWindow()
    {
        InitializeComponent();


        for (int i = 0; i < seriesCountMax; i++)
            isSeriesChosen.Add(true);

        rawCheckboxes = new List<CheckBox> { cbxData0, cbxData1, cbxData2, cbxData3, cbxData4, cbxData5 };
        normCheckboxes = new List<CheckBox> { cbxReData0, cbxReData1, cbxReData2, cbxReData3, cbxReData4, cbxReData5 };

        foreach (var cb in rawCheckboxes.Concat(normCheckboxes))
        {
            cb.Click += SeriesCheckbox_Click;
        }

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
            UpdateNormalizedChart();
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

    private void SeriesCheckbox_Click(object sender, RoutedEventArgs e)
    {
        if (!isDataLoaded) return;

        var cb = (CheckBox)sender;
        int idx;
        bool isRaw = rawCheckboxes.Contains(cb);

        if (isRaw)
            idx = rawCheckboxes.IndexOf(cb);
        else
            idx = normCheckboxes.IndexOf(cb);

        bool isChecked = cb.IsChecked == true;
        isSeriesChosen[idx] = isChecked;

        var otherCb = isRaw ? normCheckboxes[idx] : rawCheckboxes[idx];
        if (otherCb.IsChecked != isChecked)
            otherCb.IsChecked = isChecked;

        int checkedCount = isSeriesChosen.Count(x => x);
        if (checkedCount <= 2)
        {
            for (int i = 0; i < seriesCountMax; i++)
            {
                if (isSeriesChosen[i])
                {
                    rawCheckboxes[i].IsEnabled = false;
                    normCheckboxes[i].IsEnabled = false;
                }
            }
        }
        else
        {
            foreach (var c in rawCheckboxes) c.IsEnabled = true;
            foreach (var c in normCheckboxes) c.IsEnabled = true;
        }

        UpdateRawChart();
        UpdateNormalizedChart();
    }

    private void txtStartValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (isDataLoaded == false)
            tbcMainTab.SelectedIndex = 0;

        switch (tbcMainTab.SelectedIndex)
        {
            case 1:
                UpdateRawChart();
                break;

            case 2:
                UpdateNormalizedChart();
                break;

            case 3:
                UpdateZonesTable();
                break;
        }
    }

    private void txtStartValue_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isAfterInit && isDataLoaded && int.TryParse(txtStartValue.Text, out int value))
        {
            TextChanged(value);
        }
    }

    private void txtReStartValue_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isAfterInit && isDataLoaded && int.TryParse(txtReStartValue.Text, out int value))
        {
            TextChanged(value);
        }
    }


    private void TextChanged(int value)
    {

        if (value < 0)
        {
            txtStartValue.Text = "0";
            txtReStartValue.Text = "0";
        }
        else if (value > 99)
        {
            txtStartValue.Text = "99";
            txtReStartValue.Text = "99";
        }

        dataToViewStart = value;
        lblStartTime.Content = "( " + (value * rawData.DeltaTime) + "s )";
        lblReStartTime.Content = "( " + (value * rawData.DeltaTime) + "s )";

        txtReStartValue.Text = $"{value.ToString()}";
        txtStartValue.Text = $"{value.ToString()}";

        UpdateRawChart();
        UpdateNormalizedChart();
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

        rectReColor0.Fill = ConvertSkColorToBrush(seriesColors[0]);
        rectReColor1.Fill = ConvertSkColorToBrush(seriesColors[1]);
        rectReColor2.Fill = ConvertSkColorToBrush(seriesColors[2]);
        rectReColor3.Fill = ConvertSkColorToBrush(seriesColors[3]);
        rectReColor4.Fill = ConvertSkColorToBrush(seriesColors[4]);
        rectReColor5.Fill = ConvertSkColorToBrush(seriesColors[5]);
    }

    private void UpdateNormalizedChart()
    {
        if (!isDataLoaded) return;

        var allSeries = new List<ISeries>();

        for (int i = 0; i < seriesCountMax; i++)
            if (isSeriesChosen[i])
                allSeries.AddRange(rawData.GenerateNormalizedSeries(i, pointSize, dataToViewStart, seriesColors[i]));

        if (zoneLow.HasValue && zoneHigh.HasValue)
        {
            allSeries.AddRange(rawData.GenerateZoneSeries(zoneLow.Value, zoneHigh.Value, dataToViewStart, SKColors.Red));
        }

        chartNormalizedData.Series = allSeries;
    }

    private void btnHideZones_Click(object sender, RoutedEventArgs e)
    {
        if (!isDataLoaded) return;

        zoneLow = null;
        zoneHigh = null;
        UpdateNormalizedChart();
    }

    private void btnShowZones_Click(object sender, RoutedEventArgs e)
    {
        if (!isDataLoaded) return;

        if (!double.TryParse(txtZoneLow.Text.Replace(".", ","), out double low) ||
            !double.TryParse(txtZoneHigh.Text.Replace(".", ","), out double high) ||
            low < 0 || high > 1 || low >= high)
        {
            MessageBox.Show("Dozwolony zakres granic to od 0 do 1", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        zoneLow = low;
        zoneHigh = high;

        UpdateNormalizedChart();
    }

    private void DoubleOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        bool isDigit = char.IsDigit(e.Text, 0);
        bool isDot = e.Text == "." && !((TextBox)sender).Text.Contains(".");
        bool isComa = e.Text == "," && !((TextBox)sender).Text.Contains(",");
        e.Handled = !(isDigit || (isDot || isComa));
    }

    private void UpdateZonesTable()
    {
        
        if (!isDataLoaded || !zoneLow.HasValue || !zoneHigh.HasValue)
        {
            dgZones.ItemsSource = null;
            return;
        }

        var rows = new List<ZoneRow>();

        for (int i = 0; i < seriesCountMax; i++)
        {
            if (!isSeriesChosen[i]) continue;

            var firstSeries = rawData
           .GenerateNormalizedSeries(i, pointSize, dataToViewStart, seriesColors[i])
           .FirstOrDefault();

            var points = firstSeries.Values.Cast<LiveChartsCore.Defaults.ObservablePoint>();

            double? firstLow = null;
            double? firstHigh = null;

            foreach (var  p in points)
            {
                if (firstLow == null && p.Y > zoneLow.Value) firstLow = p.Y;
                if (firstHigh == null && p.Y > zoneHigh.Value) firstHigh = p.Y;

                if (firstLow.HasValue && firstHigh.HasValue)
                    break;
            }

            rows.Add(new ZoneRow
            {
                Channel = $"Kanał {i + 1}",
                Low = firstLow != null ? firstLow.Value.ToString("0.###") : "-",
                High = firstHigh != null ? firstHigh.Value.ToString("0.###") : "-"
            });
        }

        dgZones.ItemsSource = rows;
    }

    private void btnRefreshZones_Click(object sender, RoutedEventArgs e)
    => UpdateZonesTable();
}

public class ZoneRow
{
    public string Channel { get; set; }
    public string Low { get; set; }
    public string High { get; set; }
}