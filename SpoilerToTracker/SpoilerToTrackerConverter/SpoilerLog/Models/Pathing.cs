using SpoilerToTrackerConverter.SpoilerLog.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class Pathing : ICreateFromLine<Pathing>
    {
        public string? Path { get; set; }
        public int? Step {  get; set; }
        public string? Location { get; set; }
        public string? Item { get; set; }

        public Pathing CreateFromLine(string line) 
        {
            string[] parts = line.Split(':');

            string? path = "";
            string? location = "";
            string? item = "";

            if (parts.Length == 2)
            { 
                location = parts[0].Trim();
                item = parts[1].Trim();
            }

            return new Pathing
            {
                Path = path,
                Location = location,
                Item = item,
            };
        
        }
    }
}
