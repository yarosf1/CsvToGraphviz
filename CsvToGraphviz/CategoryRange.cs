using System;
using System.Collections.Generic;
using System.Globalization;

namespace CsvToGraphviz
{
    internal struct CategoryRange
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public static List<CategoryRange> FromConfig(string configValue)
        {
            var ranges = new List<CategoryRange>();
            if (!string.IsNullOrEmpty(configValue))
            {
                string[] rangesAsString = configValue.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (string rangeAsString in rangesAsString)
                {
                    string[] pair = rangeAsString.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    int minValue;
                    int maxValue;

                    int.TryParse(pair[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out minValue);
                    int.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out maxValue);

                    ranges.Add(new CategoryRange
                    {
                        MinValue = minValue,
                        MaxValue = maxValue
                    });
                }
            }
            return ranges;
        }
    }
}
