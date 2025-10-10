using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpoilerToTrackerConverter.Emotracker.Models.Items
{
    public class ItemMap
    {
        [JsonPropertyName("spoiler_label")]
        public string? SpoilerLabel { get; set; }

        [JsonPropertyName("item_reference")]
        public string? ItemReference { get; set; }

        [JsonPropertyName("values")]
        public Dictionary<string, int>? Values { get; set; }


        //Shared Items

        [JsonPropertyName("shared")]
        public string? Shared { get; set; }

        [JsonPropertyName("onvalue")]
        public int? OnValue { get; set; }

        [JsonPropertyName("item_reference2")]
        public string? ItemReference2 { get; set; }

        [JsonPropertyName("onvalue2")]
        public int? OnValue2 { get; set; }


        [JsonIgnore]
        public int? ID { get; set; }
        [JsonIgnore]
        public int? ID2 { get; set; }
        [JsonIgnore]
        public string? Type { get; set; }
        [JsonIgnore]
        public string? File { get; set; }

        [JsonIgnore]
        public string? NoIDItemReference { get; set; }
        [JsonIgnore]
        public string? NoIDItemReference2 { get; set; }
        [JsonIgnore]
        public string? RawItemReference { get; set; }
        [JsonIgnore]
        public string? RawItemReference2 { get; set; }


        [JsonIgnore]
        public string? SpecialType { get; set; }


        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(ItemReference))
                return;

            string[] parts = ItemReference.Split(':');


            // ID in front
            if (parts.Length > 1 && int.TryParse(parts[0], out int id))
            {
                ID = id;
                Type = parts[1];
                NoIDItemReference = parts[1] + parts[2];
                ItemReference = parts[2].Trim();
                RawItemReference = id + ":" + parts[1] + ":" + parts[2];
            }
            // No ID in front
            else if (parts.Length > 1)
            {
                RawItemReference = ItemReference;
                Type = parts[0];
                NoIDItemReference = ItemReference;
                ItemReference = parts[1].Trim();
            }
            // Special handling
            if (ItemReference != null)
            {
                SpecialType = ItemReference switch
                {
                    var s when s.StartsWith("Rainbow") => "BRIDGE",
                    var s when s.StartsWith("Moon") => "MOON",
                    var s when s.StartsWith("LACS") => "LACS",
                    var s when s.StartsWith("Majora") => "MAJORA",
                    var s when s.StartsWith("Triforce") => "TRIFORCE",
                    _ => SpecialType
                };
            }


            // Second Item
            if (ItemReference2 != null)
            {
                RawItemReference2 = ItemReference2;
                parts = ItemReference2.Split(":");

                if (parts.Length > 1 && int.TryParse(parts[0], out int id2)) 
                { 
                    ID2 = id2;
                    NoIDItemReference2 = parts[1] + parts[2];
                    ItemReference2 = parts[2].Trim();
                }
            }
        }
    }
}