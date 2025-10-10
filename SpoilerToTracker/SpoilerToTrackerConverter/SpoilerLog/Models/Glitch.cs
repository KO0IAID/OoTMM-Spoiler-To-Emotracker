using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class Glitch : ICreateFromLine<Glitch> , INameValueCount
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public int? Count { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public int? LogOrder { get; set; } = 0;

        public Glitch CreateFromLine(string line)
        {
            return new Glitch
            {
                Name = line,
                Value = "True",
                Description = line,
                Difficulty = null,           //Temporarily Null until a difficulty ranking system for each glitch is established
                LogOrder = LogOrder++
            };
        }
    }
}
