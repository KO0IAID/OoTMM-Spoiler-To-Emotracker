using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.Emotracker.Models.Items
{
    public class Item
    {
        #region JsonPropertyNames
        //General
        [JsonPropertyOrder(-1)][JsonPropertyName("item_reference")]
        public string? ItemReference { get; set; }


        // Progressive
        [JsonPropertyName("stage_index")]
        public int? StageIndex { get; set; }


        // Toggle & Lua
        [JsonPropertyName("active")]
        public bool? Active { get; set; }


        // Consumable
        [JsonPropertyName("acquired_count")]
        public int? AcquiredCount { get; set; }

        [JsonPropertyName("consumed_count")]
        public int? ConsumedCount { get; set; }

        [JsonPropertyName("max_count")]
        public int? MaxCount { get; set; }

        [JsonPropertyName("min_count")]
        public int? MinCount { get; set; }


        // Lua
        [JsonPropertyName("stage")]
        public double? Stage { get; set; }

        [JsonPropertyName("presetNum")]
        public double? PresetNum { get; set; }



        #endregion
        #region JsonIgnores
        [JsonIgnore]
        public string? OldValue { get; set; }
        [JsonIgnore]
        public string? NewValue { get; set; }

        [JsonIgnore]
        public int? Id { get; set; }

        [JsonIgnore]
        public string? Type { get; set; }

        [JsonIgnore]
        public string? SpecialType { get; set; }

        [JsonIgnore]
        public string? CleanItemReference { get; set; }

        [JsonIgnore]
        public string? NoIDItemReference { get; set; }
        #endregion

        [OnDeserialized]
        public void Initialize() 
        {
            if (string.IsNullOrWhiteSpace(ItemReference))
                return;

            string[] parts = ItemReference.Split(':');

            if (parts.Length >= 3 && Id == null)
            {
                if (int.TryParse(parts[0], out int id))
                    Id = id;

                Type = parts[1];
                NoIDItemReference = parts[1] + ":" + parts[2];

                CleanItemReference = Uri.UnescapeDataString(parts[2]).Replace(" ", "");
            }

            switch (Type)
            {
                case "progressive":
                    OldValue = StageIndex.ToString(); 
                    break;
                case "toggle":
                    OldValue = Active.ToString();
                    break;
                case "consumable":
                    OldValue = $"{AcquiredCount.ToString()}, {ConsumedCount.ToString()}, {MaxCount.ToString()}, {MinCount.ToString()}";
                    break;
                case "lua":
                    OldValue = $"{Active.ToString()}, {PresetNum.ToString()}";
                    break;
            }

            if (!string.IsNullOrEmpty(CleanItemReference))
            {
                SpecialType = CleanItemReference switch
                {
                    var s when s.StartsWith("Rainbow") => "BRIDGE",
                    var s when s.StartsWith("Moon") => "MOON",
                    var s when s.StartsWith("LACS") => "LACS",
                    var s when s.StartsWith("Majora") => "MAJORA",
                    var s when s.StartsWith("Triforce") => "TRIFORCE",
                    _ => SpecialType // keep old value if no match
                };
            }
        }
    }
}
