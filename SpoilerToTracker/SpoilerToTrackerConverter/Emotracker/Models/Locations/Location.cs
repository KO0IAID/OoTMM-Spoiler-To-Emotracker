using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace SpoilerToTrackerConverter.Emotracker.Models.Locations
{
    public class Location
    {
        [JsonIgnore]
        public int? Id { get; set; }

        [JsonIgnore]
        public string? Acronym { get; set; }

        [JsonIgnore]
        public string? CleanItemReference { get; set; }

        [JsonPropertyName("location_reference")]
        public string? LocationReference { get; set; }

        [JsonPropertyName("modified_by_user")]
        public bool? ModifiedByUser { get; set; }

        [JsonPropertyName("sections")]
        public List<Section>? Sections { get; set; }
    
        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(LocationReference))
                return;

            string[] parts = LocationReference.Split(':');
            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[0], out int id))
                    Id = id;

                Acronym = Uri.UnescapeDataString(parts[1]).Replace(" ", "").Substring(0,2);
                CleanItemReference = Uri.UnescapeDataString(parts[1]);
                CleanItemReference = CleanItemReference.Replace(" ", "");
            }

            if (Sections != null)
            {
                foreach (var section in Sections)
                {
                    section.Initialize();
                }
            }
        }

    }
}
