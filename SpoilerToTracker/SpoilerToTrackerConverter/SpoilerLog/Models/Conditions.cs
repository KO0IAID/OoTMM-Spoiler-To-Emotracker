using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    // Named to Conditions Instead of Condition to avoid ambiguity of System.Windows.Condition
    public class Conditions : INameValueCount
    {
        public string? Type {  get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
        public int? Count { get; set; }

        public Conditions(string? type, string? name, string? value, int? count = null)
        {
            Type = type;
            Name = name;
            Value = value;
            Count = count;
        }
    }
}
