using SpoilerToTrackerConverter.SpoilerLog.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.SpoilerLog.Models
{
    public class JunkLocation : ICreateFromLine<JunkLocation>
    {
       public string? Location { get; set; }

       public JunkLocation CreateFromLine(string line) 
       {

            return new JunkLocation
            {
                Location = line
            };
       }

    }
}
