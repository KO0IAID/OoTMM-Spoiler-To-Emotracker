using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class ItemLocation : ICreateFromLine<ItemLocation>
    {
        public string? World { get; set; }
        public string? Game { get; set; }
        public string? Region { get; set; }
        public int? Count { get; set; }
        public int? Number { get; set; }
        public string? Player { get; set; }
        public string? Description { get; set; }
        public string? Item { get; set; }

        public ItemLocation CreateFromLine(string line) 
        {
            string? world = null;
            string? game = null;
            string? region = null;
            int? count = null;
            string? player = null;
            string? description = null;
            string? item = null;

            // Game 
            if (line.Trim().StartsWith("OOT"))
            {
                // Assigns Game
                game = "OOT";

                // Remove Event Prefix
                line = line.Replace("OOT", "").Trim();
            }
            else if (line.Trim().StartsWith("MM"))
            {
                // Assigns Type
                game = "MM";

                // Remove Location Prefix
                line = line.Replace("MM", "").Trim();
            }

            // Player
            if (line.Contains("Player"))
            {
                Match match = Regex.Match(line, @"(Player \d+)");

                if (match.Success)
                {
                    // Assign Player
                    player = match.Groups[1].Value;

                    // Removes Player Portion from line
                    line = Regex.Replace(line, @"Player \d+\s*", "").Trim();
                }
            }

            // Description & Itempoly
            if (line.Contains(':'))
            {
                string[] parts = line.Split(':');

                if (parts.Length >= 1)
                {
                    // Assigns Location & Itempoly
                    description = parts[0].Trim();

                    if (parts[1].Contains("()"))
                    {
                        item = parts[1].Replace("()", "").Trim();
                    }
                    else 
                    {
                        item = parts[1].Trim();
                    }
                }
            }


            return new ItemLocation
            {
                World = world,
                Game = game,
                Region = region,
                Count = count,
                Player = player,
                Description = description,
                Item = item
            };
        }

    }
}
