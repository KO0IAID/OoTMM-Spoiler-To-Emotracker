using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class Entrance
    {
        public string? World { get; set; }
        public string? FromGame {  get; set; }
        public string? ToGame { get; set; }
        public string? LongEntrance { get; set; }
        public string? LongDestination { get; set; }
        public string? ShortEntrance { get; set; }
        public string? ShortDestination { get; set; }
        public int? LogOrder { get; set; }
    }
}
