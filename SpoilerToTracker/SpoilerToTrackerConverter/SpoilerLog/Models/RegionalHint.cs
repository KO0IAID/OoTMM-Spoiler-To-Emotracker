using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class RegionalHint : ICreateFromLine <RegionalHint>
    {
        public string? World { get; set; }
        public string? GossipStone {  get; set; }
        public string? Region { get; set; }
        public string? Item { get; set; }

        public RegionalHint CreateFromLine(string line)
        {
            string[] parts = Regex.Split(line.Trim(), @" {2,}");

            if (parts.Length != 3)
                throw new FormatException($"Region Hint Line Couldn't CreateFromLine at line: '{line}'");

            return new RegionalHint
            {
                GossipStone = parts[0],
                Region = parts[1],
                Item = parts[2]
            };
        }
    }
}
