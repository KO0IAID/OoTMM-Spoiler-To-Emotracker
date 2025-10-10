using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.Emotracker.Models.Locations
{
    public class Section
    {
        [JsonIgnore]
        public string? Acronym { get; set; }

        [JsonIgnore]
        public string? CleanSectionReference { get; set; }

        [JsonPropertyName("section_reference")]
        public string? SectionReference { get; set; }

        [JsonPropertyName("available_chest_count")]
        public int? AvailableChestCount { get; set; }

        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(SectionReference))
                return;

            string[] parts = SectionReference.Split(':');
            if (parts.Length >= 2)
            {

                Acronym = Uri.UnescapeDataString(parts[1]).Replace(" ", "").Substring(0,2);
                CleanSectionReference = Uri.UnescapeDataString(parts[1]);
                CleanSectionReference = CleanSectionReference.Replace(" ","");
            }
        }

    }
}
