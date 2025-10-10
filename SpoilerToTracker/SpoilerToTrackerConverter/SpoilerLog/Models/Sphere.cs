using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class Sphere : ICreateFromLine<Sphere>
    {
        public string? Type { get; set; }
        public int? Number { get; set; }
        public string? World { get; set; }
        public string? Location { get; set; }
        public string? Player { get; set; }
        public string? Item { get; set; }

        public Sphere CreateFromLine(string line) 
        {
            string? type = null;
            int? number = null;
            string? world = null;
            string? location = null;
            string? player = null;
            string? item = null;

            // Type 
            if (line.Trim().StartsWith("Event -"))
            {
                // Assigns Type
                type = "Event";

                // Remove Event Prefix
                line = line.Replace("Event -", "").Trim();
            }
            else if (line.Trim().StartsWith("Location -"))
            {
                // Assigns Type
                type = "Location";

                // Remove Location Prefix
                line = line.Replace("Location -", "").Trim();
            }

            // World
            if (line.StartsWith("World"))
            {
                Match worldMatch = Regex.Match(line, @"^(World \d+)\s+(.*)$");

                if (worldMatch.Success)
                {
                    // Assigns World
                    world = worldMatch.Groups[1].Value;

                    // Removes World Prefix
                    line = worldMatch.Groups[2].Value;   
                }
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

            // Location & Itempoly
            if (line.Contains(':'))
            {
                string[] parts = line.Split(':');

                if (parts.Length >= 1)
                { 
                    // Assigns Location & Itempoly
                    location = parts[0].Trim();
                    item = parts[1].Trim();
                }
            }
            else 
            { 
                // Assigns Location (It would be an event location)
                location = line;
            }

            return new Sphere
            {
                Type = type,
                Number = number,
                World = world,
                Location = location,
                Player = player,
                Item = item
            };
        }
    }
}
