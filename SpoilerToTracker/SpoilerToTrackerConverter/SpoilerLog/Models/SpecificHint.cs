using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class SpecificHint : ICreateFromLine<SpecificHint>
    {
        public string? World { get; set; }
        public string? GossipStone { get; set; }
        public string? Location { get; set; }
        public string? Item { get; set; }
        
        public SpecificHint CreateFromLine(string line)
        {
            // Split line into up to 3 parts on 2+ spaces
            string[] parts = Regex.Split(line.Trim(), @" {2,}")
                                    .Where(p => !string.IsNullOrWhiteSpace(p))
                                    .ToArray();

            if (parts.Length == 3)
            {
                return new SpecificHint
                {
                    GossipStone = parts[0],
                    Location = parts[1],
                    Item = parts[2]
                };
            }
            else if (parts.Length == 2)
            {
                // No Gossip Stone provided — continuation from previous
                return new SpecificHint
                {
                    GossipStone = "", // handle this in context
                    Location = parts[0],
                    Item = parts[1]
                };
            }
            else
            {
                throw new FormatException($"Specific Hint couldn't CreateFromLine at line: '{line}'");
            }
        
        }
    }
}
