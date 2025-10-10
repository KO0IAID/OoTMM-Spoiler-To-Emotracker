using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class FoolishRegion : ICreateFromLine<FoolishRegion>
    {
        public string? World { get; set; }
        public string? Region { get; set; }
        public int? Count { get; set; }

        public FoolishRegion CreateFromLine(string line)
        {
            string[] parts = line.Split(':');

            return new FoolishRegion
            {
                Region = parts[0].Trim(),
                Count = int.Parse(parts[1].Trim())
            };  
        }
    }
}
