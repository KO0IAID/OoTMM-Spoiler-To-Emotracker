using SpoilerToTrackerConverter.SpoilerLog.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class Dungeon : ICreateFromLine<Dungeon>, INameValueCount
    {
        public string? Name { get; set; }
        public string? Value {  get; set; }
        public int? Count { get; set; }
        public int? TrackerOrder { get; set; }


        public Dungeon CreateFromLine(string line) 
        {
            string name = "";
            int trackerOrder = 0;
            switch (line)
            {
                case "DT":
                    name = "Deku Tree";
                    trackerOrder = 6;
                    break;
                case "DC":
                    name = "Dodongos Cavern";
                    trackerOrder = 7;
                    break;
                case "JJ":
                    name = "Jabu-Jabu's Belly";
                    trackerOrder = 8;
                    break;
                case "Forest":
                    name = "Forest Temple";
                    trackerOrder = 1;
                    break;
                case "Fire":
                    name = "Fire Temple";
                    trackerOrder = 2;
                    break;
                case "Water":
                    name = "Water Temple";
                    trackerOrder = 3;
                    break;
                case "Shadow":
                    name = "Shadow Temple";
                    trackerOrder = 5;
                    break;
                case "Spirit":
                    name = "Spirit Temple";
                    trackerOrder = 4;
                    break;
                case "WF":
                    name = "Woodfall Temple";
                    trackerOrder = 9;
                    break;
                case "SH":
                    name = "Snowhead Temple";
                    trackerOrder = 10;
                    break;
                case "GB":
                    name = "Great Bay Temple";
                    trackerOrder = 11;
                    break;
                case "ST":
                    name = "Stone Tower Temple";
                    trackerOrder = 12;
                    break;
            }
            return new Dungeon
            {
                
                Name = name,
                Value = "true",
                TrackerOrder = trackerOrder
            };
        }
    }
}
