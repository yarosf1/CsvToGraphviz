using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CsvToGraphviz
{
    public class Program
    {
        private static readonly bool _showDescriptions = GetIfShowDescriptions();
        private static readonly double _valueTreshold = GetValueTreshold();
        private static readonly List<CategoryRange> _ranges = CategoryRange.FromConfig(ConfigurationManager.AppSettings["SkipRangesWithinCategory"]);

        internal static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["CsvSearchMask"]);

            foreach (string path in files)
            {
                Console.WriteLine("Processing file: {0}", path);
                Operate(path);
            }
        }

        private static void Operate(string path)
        {
            var rawData = new List<string[]>();

            using (var reader = new StreamReader(File.OpenRead(path), GetInputEncoding()))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        string[] values = line.Split(ConfigurationManager.AppSettings["CsvSeparator"].ToCharArray());
                        rawData.Add(values);
                    }
                }
            }

            string fileName = Path.GetFileNameWithoutExtension(path);

            string[] captions = rawData.First().ToArray();

            string[][] numericData = rawData.Skip(1).ToArray();

            var builder = new StringBuilder();

            builder.AppendFormat(@"graph {0} {{{1}", fileName, Environment.NewLine);

            for (int rowIndex = 0; rowIndex < numericData.Length; rowIndex++)
            {
                string[] row = numericData[rowIndex];
                for (int columnIndex = rowIndex + 1; columnIndex < row.Length; columnIndex++)
                {
                    string cell = row[columnIndex].Replace(ConfigurationManager.AppSettings["DecimalSeparator"], ".");
                    double cellData;
                    double.TryParse(cell, NumberStyles.Any, CultureInfo.InvariantCulture, out cellData);

                    if (IsInRange(cellData) && !IsWithinCategory(columnIndex+1, rowIndex+1))
                    {
                        AppendGraphLine(builder, rowIndex, columnIndex, cellData, captions);
                    }
                }
            }

            builder.AppendLine("}");

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(path), fileName + ".txt"), builder.ToString(), Encoding.UTF8);
        }

        private static Encoding GetInputEncoding()
        {
            int codepage;
            return int.TryParse(ConfigurationManager.AppSettings["InputEncodingCodepage"], NumberStyles.Any, CultureInfo.InvariantCulture, out codepage)
                ? Encoding.GetEncoding(codepage)
                : Encoding.UTF8;
        }

        private static bool IsInRange(double data)
        {
            return Math.Abs(data) > _valueTreshold;
        }

        private static bool IsWithinCategory(int column, int row)
        {
            return _ranges.Any(range => column >= range.MinValue && column <= range.MaxValue
                && row >= range.MinValue && row <= range.MaxValue);
        }

        private static void AppendGraphLine(StringBuilder builder, int rowIndex, int columnIndex, double cellData, string[] captions)
        {
            var extraAttributes = new List<string>();
            if (_showDescriptions)
            {
                extraAttributes.Add($@"label=""{cellData.ToString(ConfigurationManager.AppSettings["DescriptionValueFormat"])}""");
            }
            if (cellData < 0)
            {
                extraAttributes.Add("style=dotted");
            }
            string attributes = extraAttributes.Count > 0 ? $@" [{string.Join(" ", extraAttributes)}]" : string.Empty;

            //Отс_од_ -- Поворот_ф_ [style=dotted];
            builder.AppendFormat(@"    ""{0}"" -- ""{1}""{2};{3}",
                captions[rowIndex],
                captions[columnIndex],
                attributes,
                Environment.NewLine);
        }

        private static bool GetIfShowDescriptions()
        {
            bool value;
            bool.TryParse(ConfigurationManager.AppSettings["ShowDescriptions"], out value);
            return value;
        }

        private static double GetValueTreshold()
        {
            double value;
            double.TryParse(ConfigurationManager.AppSettings["ValueTreshold"], NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            return value;
        }
    }
}
