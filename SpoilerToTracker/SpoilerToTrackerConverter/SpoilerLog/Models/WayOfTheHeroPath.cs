using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class WayOfTheHeroPath : ICreateFromLine<WayOfTheHeroPath>
    {
        public string? World {  get; set; }
        public string? Description { get; set; }
        public string? Player {  get; set; }
        public string? Item { get; set; }
        public int? LogOrder { get; set; }

        public WayOfTheHeroPath CreateFromLine(string line) 
        {
            if (string.IsNullOrWhiteSpace(line))
                throw new ArgumentException("Input line cannot be null or whitespace.");

            // Split once at the first colon
            string[] parts = Regex.Split(line, @"\s*:\s*", RegexOptions.None, TimeSpan.FromMilliseconds(100));

            if (parts.Length != 2)
                throw new FormatException("Line format invalid. Expected a single ':' separating description and item.");

            string leftPart = parts[0].Trim();  // before colon
            string rightPart = parts[1].Trim(); // after colon

            string? world = null;
            string? player = null;
            string description = leftPart;
            string item = rightPart;

            // Match optional World prefix in the left part
            var worldMatch = Regex.Match(description, @"^World\s+\d+\b", RegexOptions.IgnoreCase);
            if (worldMatch.Success)
            {
                world = worldMatch.Value.Trim();
                description = description.Substring(worldMatch.Length).Trim();
            }

            // Match optional Player prefix in the right part
            var playerMatch = Regex.Match(item, @"^Player\s+\d+\b", RegexOptions.IgnoreCase);
            if (playerMatch.Success)
            {
                player = playerMatch.Value.Trim();
                item = item.Substring(playerMatch.Length).Trim();
            }

            return new WayOfTheHeroPath
            {
                World = world,
                Player = player,
                Description = description,
                Item = item,
                LogOrder = LogOrder++
            };
        }
    }
}
