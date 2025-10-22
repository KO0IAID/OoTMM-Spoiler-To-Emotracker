using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class DungeonReward
    {
        public string? Dungeon { get; set; }
        public string? Reward { get; set; }
        public int? Order { get; set; }
        public string? Note {  get; set; }
    }
}
