using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class StartingItem : ICreateFromLine<StartingItem>, INameValueCount
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public int? Count { get; set; }

        public StartingItem CreateFromLine(string line)
        {

            if (string.IsNullOrWhiteSpace(line))
                throw new ArgumentException("Line cannot be null or empty.", nameof(line));

            string[] parts = line.Split(':');
            if (parts.Length < 2)
                throw new FormatException("Line must be in format 'Name:Count'");

            string name = parts[0].Trim();

            if (!int.TryParse(parts[1], out int count))
                throw new FormatException($"Invalid number format: {parts[1]}");

            string value = count > 0 ? "true" : "false";


            return new StartingItem
            {
                Name = name,
                Value = value,
                Count = count
            };
        }
    }
}
