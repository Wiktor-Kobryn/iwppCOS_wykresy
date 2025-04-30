using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace IwppCOSwykresy
{
    public class RawData
    {
        public List<List<double>> dataSeries = new List<List<double>>();
        public List<double> time = new List<double>();

        private int seriesCountMax = 6;
        private int seriesStartColIndex = 2;
        private int seriesLength = 0;
        public float DeltaTime { get; private set; } = 0.0f;
        public float StartTime { get; private set; } = 0.0f;

        public RawData()
        {
            for(int i = 0; i < seriesCountMax; i++)
            {
                dataSeries.Add(new List<double>());
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
                // skip title
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line?.Split(';');

                    // parse data
                    for (int i = 0; i < seriesCountMax; i++)
                    {
                        double conductivity = Convert.ToDouble(values[i + seriesStartColIndex], new CultureInfo("pl-PL"));
                        dataSeries[i].Add(conductivity);

                        seriesLength++;
                    }
                }
            }
        }

        private void ReadTimeFromAFile(string filePath)
        {
            DateTime[] timeToCompare = new DateTime[2];

            using (var reader = new StreamReader(filePath))
            {
                // skip title & first 2 rows
                for (int i = 0; i < 3; i++)
                    reader.ReadLine();

                // read time on 2 next lines and compare
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
    }
}
