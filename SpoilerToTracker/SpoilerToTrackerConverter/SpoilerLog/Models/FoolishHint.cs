using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class FoolishHint : ICreateFromLine<FoolishHint>
    {
        public string? World {  get; set; }
        public string? GossipStone { get; set; }
        public string? Location { get; set; }

        public FoolishHint CreateFromLine(string line) 
        {
            string[] parts = Regex.Split(line.Trim(), @"\s{2,}")
                          .Where(p => !string.IsNullOrWhiteSpace(p))
                          .ToArray();

            if (parts.Length != 2)
            {
                throw new FormatException($"Foolish couldn't CreateFromLine at line: '{line}'");
            }

            return new FoolishHint
            {
                GossipStone = parts[0],
                Location = parts[1],
            };
        }
    }
}
