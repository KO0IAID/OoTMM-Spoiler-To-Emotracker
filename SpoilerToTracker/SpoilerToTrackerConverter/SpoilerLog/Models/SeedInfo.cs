using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class SeedInfo : ICreateFromLine<SeedInfo>
    {
        public string? Name { get; set; }
        public string? Value { get; set; }

        public KeyValuePair<string, string>? Pair
        {
            get => (Name != null && Value != null)
                ? new KeyValuePair<string, string>(Name, Value)
                : (KeyValuePair<string, string>?)null;

            set
            {
                if (value.HasValue)
                {
                    Name = value.Value.Key;
                    Value = value.Value.Value;
                }
                else
                {
                    Name = null;
                    Value = null;
                }
            }
        }


        public SeedInfo CreateFromLine(string line)
        {
            string[] parts = line.Split(':');

            return new SeedInfo
            {
                Name = parts[0].Trim(),
                Value = parts[1].Trim(),
            };
        }
    }
}
