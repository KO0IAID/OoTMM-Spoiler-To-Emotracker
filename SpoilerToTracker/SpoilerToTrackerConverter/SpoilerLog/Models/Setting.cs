using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class Setting : ICreateFromLine<Setting>, INameValueCount
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public int? Count { get; set; }
        public int LogOrder { get; set; } = 0;

        public Setting CreateFromLine(string line)
        {
            string[] parts = line.Split(':');

            if (parts.Length < 2)
            {
                throw new ArgumentException($"Invalid format: {line}");
            }

            string name = parts[0].Trim();
            string value = parts[1].Trim();

            int? count = null;
            if (int.TryParse(value, out int parsedCount))
            {
                count = parsedCount;
            }

            return new Setting
            {
                Name = name,
                Value = value,
                Count = count,
                LogOrder = LogOrder++
            };
        }
    }
}
