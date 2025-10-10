using SpoilerToTrackerConverter.Emotracker.Models.Items;
using SpoilerToTrackerConverter.Emotracker.Models.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.Emotracker.Models
{
    public class JsonTracker
    {
        [JsonPropertyName("package_uid")]
        public string? PackageUID { get; set; }

        [JsonPropertyName("package_variant_uid")]
        public string? PackageVariantUID { get; set; }

        [JsonPropertyName("package_version")]
        public string? PackageVersion { get; set; }

        [JsonPropertyName("creation_time")]
        public string? CreationTime { get; set; }

        [JsonPropertyName("ignore_all_logic")]
        public bool? IgnoreAllLogic { get; set; }

        [JsonPropertyName("display_all_locations")]
        public bool? DisplayAllLocations { get; set; }

        [JsonPropertyName("always_allow_chest_manipulation")]
        public bool? AlwaysAllowChestManipulation { get; set; }

        [JsonPropertyName("auto_unpin_locations_on_clear")]
        public bool? AutoUnpinLocationsOnClear { get; set; }

        [JsonPropertyName("pin_locations_on_item_capture")]
        public bool? PinLocationsOnItemCapture { get; set; }

        [JsonPropertyName("item_database")]
        public List<Item>? ItemDatabase { get; set; }

        [JsonPropertyName("location_database")]
        public LocationDatabase? LocationDatabase { get; set; }

        [JsonPropertyName("main_window_width")]
        public double? MainWindowWidth { get; set; }

        [JsonPropertyName("main_window_height")]
        public double? MainWindowHeight { get; set; }

    }   
}

