using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IwppCOSwykresy
{
    public class RawData
    {
        public List<List<double>> dataSeries = new List<List<double>>();

        private int seriesCountMax = 6;
        private int seriesStartColIndex = 2;

        public RawData()
        {
            for(int i = 0; i < seriesCountMax; i++)
            {
                dataSeries.Add(new List<double>());
            }
        }

        public void ReadFromAFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                // skip title
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line?.Split(';');
                    
                    for(int i = 0; i < seriesCountMax; i++)
                    {
                        dataSeries[i].Add(Convert.ToDouble(values[i + seriesStartColIndex], new CultureInfo("pl-PL")));
                    }
                }
            }
        }
    }
}
