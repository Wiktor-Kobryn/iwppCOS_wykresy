﻿using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Globalization;
using System.IO;


namespace IwppCOSwykresy
{
    public class RawData
    {
        public List<List<double>> dataSeries = new List<List<double>>();
        public List<List<double>> dataNormalizedSeries = new List<List<double>>();
        public List<double> time = new List<double>();

        private int seriesCountMax = 6;
        private int seriesStartColIndex = 2;
        private int seriesLength = 0;
        private string dataName = "Kanał";

        public float DeltaTime { get; private set; } = 0.0f;
        public float StartTime { get; private set; } = 0.0f;

        public RawData(int seriesCountMax)
        {
            this.seriesCountMax = seriesCountMax;

            for (int i = 0; i < seriesCountMax; i++)
            {
                dataSeries.Add(new List<double>());
                dataNormalizedSeries.Add(new List<double>());
            }
        }

        public void ReadFromAFile(string filePath)
        {
            ReadDataFromAFile(filePath);
            ReadTimeFromAFile(filePath);
        }

        private void ReadDataFromAFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line?.Split(';');

                    for (int i = 0; i < seriesCountMax; i++)
                    {
                        double conductivity = Convert.ToDouble(values[i + seriesStartColIndex], new CultureInfo("pl-PL"));
                        dataSeries[i].Add(conductivity);
                    }

                    seriesLength++;
                }
            }
        }

        private void ReadTimeFromAFile(string filePath)
        {
            DateTime[] timeToCompare = new DateTime[2];

            using (var reader = new StreamReader(filePath))
            {
                for (int i = 0; i < 3; i++)
                    reader.ReadLine();

                for (int i = 0; i < 2; i++)
                {
                    if (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line?.Split(';');

                        string timeText = values[1];
                        timeToCompare[i] = DateTime.ParseExact(timeText, "yyyy-MM-dd HH:mm:ss,f", new CultureInfo("pl-PL"));
                    }
                }
            }

            DeltaTime = timeToCompare[1].Second - timeToCompare[0].Second;
            CalculateDataTime();
        }

        private void CalculateDataTime()
        {
            float currentTime = StartTime;

            for (int i = 0; i < seriesLength; i++)
            {
                time.Add(currentTime);
                currentTime += DeltaTime;
            }
        }

        public IEnumerable<ISeries> GenerateSeries(int index, double pointSize, int startIndex, SKColor color)
        {
            var values = new List<ObservablePoint>();
            for (int i = startIndex; i < seriesLength; i++)
                values.Add(new ObservablePoint(time[i], dataSeries[index][i]));

            return new ISeries[]
            {
                new ScatterSeries<ObservablePoint>
                {
                    Values = values,
                    Name = dataName + " " + (index + 1),
                    GeometrySize = pointSize,
                    Fill = new SolidColorPaint(color),
                    Stroke = new SolidColorPaint(color)
                }
            };
        }

        public List<double> MaxValues { get; private set; } = [];

        public IEnumerable<ISeries> GenerateNormalizedSeries(int index, double pointSize, int startIndex, SKColor color)
        {
            var series = dataSeries[index].Skip(startIndex).ToList();

            int count = series.Count;
            if (count == 0) yield break;

            double C0 = series.Min();
            double Cx = series.Max();
            double denom = Cx - C0;
            if (Math.Abs(denom) < 1e-10)
                denom = 1;

            var values = new List<ObservablePoint>();
            for (int i = startIndex; i < count; i++)
            {
                double Ct = series[i];
                double Cb = (Ct - C0) / denom;

                dataNormalizedSeries[index].Add(Math.Round(Cb, 7));

                values.Add(new ObservablePoint(time[i], Cb));
            }

            yield return new ScatterSeries<ObservablePoint>
            {
                Values = values,
                Name = $"{dataName} {index + 1}",
                GeometrySize = pointSize,
                Fill = new SolidColorPaint(color),
                Stroke = new SolidColorPaint(color)
            };
        }

        public IEnumerable<ISeries> GenerateZoneSeries(double low, double high, int startIndex, SKColor color, float thickness = 2f, float dashLength = 5f)
        {
            if (time.Count <= startIndex) yield break;

            double t0 = time[startIndex];
            double t1 = time.Last();

            yield return new LineSeries<ObservablePoint>
            {
                Values = new[]
                {
                new ObservablePoint(t0, low),
                new ObservablePoint(t1, low)
            },
                GeometrySize = 0,
                Stroke = new SolidColorPaint(color) { StrokeThickness = thickness },
                Fill = null
            };

            yield return new LineSeries<ObservablePoint>
            {
                Values = new[]
                {
                new ObservablePoint(t0, high),
                new ObservablePoint(t1, high)
            },
                GeometrySize = 0,
                Stroke = new SolidColorPaint(color) { StrokeThickness = thickness },
                Fill = null
            };
        }
    }
}
