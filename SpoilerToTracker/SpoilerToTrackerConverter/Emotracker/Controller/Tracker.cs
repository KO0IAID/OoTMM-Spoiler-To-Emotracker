using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;
using SpoilerToTrackerConverter.SpoilerLog.Models;
using SpoilerToTrackerConverter.SpoilerLog.Controller;
using SpoilerToTrackerConverter.Emotracker.Models;
using SpoilerToTrackerConverter.Emotracker.Models.Items;



namespace SpoilerToTrackerConverter.Emotracker.Controller
{
    public class Tracker
    {
        public JsonTracker? JsonTracker { get; private set; }
        public Spoiler? Spoiler { get; private set; }
        public List<ItemMap>? Maps { get; private set; }
        public List<string> SharedItems { get; private set; } = new();
        public int? ChangeCount { get; private set; }
        public string? ChangeLog { get; private set; }
        public bool DebugStats { get; private set; }

        public bool Converted { get; private set; }

        #region Core Exposed Functionality
        public async Task ConvertSpoilerToEmotracker(string spoilerPath, string outputPath, bool showDebug = false) 
        {
            ChangeCount = 0;
            Converted = false;
            Stopwatch sw = Stopwatch.StartNew();

            string? TemplatePath = GetTemplatePath();
            string[]? MapPaths = GetMapPaths();

            if (TemplatePath != null && MapPaths != null)
            {
                await ImportSpoiler(spoilerPath);
                await LoadTemplate(TemplatePath);
                await LoadMaps(MapPaths);
                await UpdateTracker(showDebug);
                await ExportTracker(outputPath);

                if (showDebug) { DisplayDebugStats(sw); }
            }
            

        }
        public async Task ConvertSpoilerToEmotracker(Spoiler spoiler, string outputPath, bool showDebug = false)
        {
            ChangeCount = 0;
            Converted = false;
            Stopwatch sw = Stopwatch.StartNew();

            string? TemplatePath = GetTemplatePath();
            string[]? MapPaths = GetMapPaths();

            if (TemplatePath != null && MapPaths != null)
            {
                await ImportSpoiler(null, spoiler);
                await LoadTemplate(TemplatePath);
                await LoadMaps(MapPaths);
                await UpdateTracker(showDebug);
                await ExportTracker(outputPath);

                if (showDebug) { DisplayDebugStats(sw); }
            }
        }
        #endregion
        #region File Imports
        private async Task ImportSpoiler(string? filePath = null, Spoiler? spoiler = null)
        {
            if (filePath == null && spoiler == null)
            {
                Debug.WriteLine("ImportSpoiler: Neither a filePath or spoiler object with value were provided.");
                return;
            }

            if (spoiler != null)
            {
                Spoiler = spoiler;
            }
            else if (filePath != null)
            {
                Spoiler = new Spoiler();
                await Spoiler.AddFileContents(filePath);

                if (Spoiler == null)
                {
                    Debug.WriteLine("Failed to Import Spoiler");
                }
            }
        }
        private static string? GetTemplatePath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string templateFolder = Path.Combine(baseDir, "Emotracker", "Resources", "Tracker Template");

            if (!Directory.Exists(templateFolder))
            {
                Debug.WriteLine($"Template folder not found: {templateFolder}");
                return null;
            }

            var jsonFiles = Directory.GetFiles(templateFolder, "*.json");

            if (jsonFiles.Length == 0)
            {
                Debug.WriteLine($"No JSON template found in {templateFolder}");
                return null;
            }
            else if (jsonFiles.Length > 1)
            {
                Debug.WriteLine($"Multiple JSON templates found in {templateFolder}, please keep only one.");
                return null;
            }

            return jsonFiles[0];
        }
        private static string[] GetMapPaths()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string mapDir = Path.Combine(baseDir, "Emotracker", "Resources", "Maps");

            if (!Directory.Exists(mapDir))
            {
                Debug.WriteLine($"Map directory not found: {mapDir}");
                return Array.Empty<string>();
            }

            return Directory.GetFiles(mapDir, "*.json", SearchOption.TopDirectoryOnly);
        }



        private async Task LoadTemplate(string filePath)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            string pathToUse = filePath;

            if (!File.Exists(pathToUse))
                throw new FileNotFoundException("JsonTracker file not found.", pathToUse);

            // Open the JSON file for reading
            using var stream = File.OpenRead(pathToUse);

            // Register custom converter for Itempoly
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Deserialize the tracker JSON using your JsonTracker class
            JsonTracker = await JsonSerializer.DeserializeAsync<Models.JsonTracker>(stream, options);

            if (JsonTracker == null)
                return;

            if (JsonTracker.ItemDatabase != null)
            {
                foreach (Item item in JsonTracker.ItemDatabase)
                {
                    item?.Initialize();
                }
            }
        }
        private async Task LoadMaps(string[] filePaths)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Maps = new List<ItemMap>(); // Reset master list

            var mapOptions = new JsonSerializerOptions  {   PropertyNameCaseInsensitive = true  };

            foreach (string filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    Debug.WriteLine($"File not found: {filePath}");
                    continue;
                }

                try
                {
                    using FileStream stream = File.OpenRead(filePath);
                    var items = await JsonSerializer.DeserializeAsync<List<ItemMap>>(stream, mapOptions);

                    if (items == null)
                    {
                        Debug.WriteLine($"Failed to deserialize JSON file: {filePath}");
                        continue;
                    }

                    foreach (var item in items)
                    {
                        item.Initialize();
                        item.File = filePath;
                        Maps.Add(item);
                    }
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"JSON error in file '{filePath}': {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing file '{filePath}': {ex.Message}");
                }
            }     
        }
        #endregion
        #region Mapping Processes
        private async Task UpdateTracker(bool debugStats = false)
        {
            await Task.Run(() =>
            {
                if (Spoiler == null || Maps == null || JsonTracker == null)
                {
                    Debug.WriteLine("\n--------- JsonTracker FAILED to Update! ---------");
                    Debug.WriteLineIf(Spoiler == null, "Spoilerlog is null/empty");
                    Debug.WriteLineIf(Maps == null, "Maps is null/empty");
                    Debug.WriteLineIf(JsonTracker == null, "JsonTracker is null/empty");
                    return;
                }

                MapGameSettings();
                AddSharedSettings();
                MapSpecialConditions();
                MapItems(Spoiler.Tricks, "Tricks");
                MapItems(Spoiler.PreCompletedDungeons, "Pre-Completed Dungeons");
                MapSharedItems();
                MapItems(Spoiler.StartingItems, "StartingItems");
                MapDungeonRewards();
            });
        }
        private void AddSharedSettings()
        {

            if (Spoiler == null || Spoiler.GameSettings == null || Maps == null || JsonTracker?.ItemDatabase == null)
                return;

            // get setting
            foreach (Setting entry in Spoiler.GameSettings)
            {
                string? entryName = entry.Name;
                string? entryValue = entry.Value;
                int? entryCount = entry.Count;

                if (entryName == null) continue;
                // get Map
                foreach (ItemMap itemMap in Maps)
                {
                    string? mapName = itemMap.NoIDItemReference;
                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapType = itemMap.Type;
                    string? mapShared = itemMap.Shared;

                    if (entryName == mapShared) 
                    {
                        if (entryValue == "true" && entryName != null && !SharedItems.Contains(entryName))
                        {
                            SharedItems.Add(entryName);
                            ChangeLog += $"Shared:\t{entryName}\t\t\t Added to SharedMaps\n";
                        }
                    }
                }
            }
        }
        private void MapGameSettings()
        {
            if (Spoiler == null || Spoiler.GameSettings == null || Maps == null || JsonTracker?.ItemDatabase == null)
                return;

            // get Spoiler log item
            foreach (Setting entry in Spoiler.GameSettings)
            {
                string? entryName = entry.Name;
                string? entryValue = entry.Value;
                int? entryCount = entry.Count;

                // get Map
                foreach (ItemMap itemMap in Maps)
                {
                    string? mapName = itemMap.NoIDItemReference;
                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapType = itemMap.Type;

                    // Compare spoilerlog item to map
                    if (string.Equals(entryName, mapSpoilerLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (Item item in JsonTracker.ItemDatabase)
                        {
                            string? itemName = item.NoIDItemReference;
                            string? itemType = item.Type;

                            // Compare map to item
                            if (mapName == itemName && mapType == itemType)
                            {

                                if (entryValue != null && itemMap.Values != null)
                                {
                                    itemMap.Values.TryGetValue(entryValue, out int mappedValue);

                                    switch (itemType)
                                    {
                                        case "progressive":
                                            if (mappedValue != item.StageIndex)
                                            {
                                                item.StageIndex = mappedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "toggle":
                                            bool newToggleValue = mappedValue <= 0;
                                            if (newToggleValue != item.Active)
                                            {
                                                item.Active = newToggleValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "toggle_badged":
                                            bool newBadgedValue = mappedValue <= 0;
                                            if (newBadgedValue != item.Active)
                                            {
                                                item.Active = newBadgedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                        case "lua":
                                            bool newLuaValue = mappedValue > 0;
                                            if (item.Active != newLuaValue)
                                            {
                                                item.Active = newLuaValue;
                                                item.Stage = mappedValue;
                                                item.NewValue = newLuaValue.ToString() + "," + mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "consumable":
                                            if (entryCount != item.AcquiredCount)
                                            {
                                                item.AcquiredCount = entryCount;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    Debug.WriteLineIf(DebugStats,
                                        $"*** BAD MAP VALUES ***\n" +
                                        $"Source:\tMapGameSettings\n" +
                                        $"Item:\t{itemName}\n" +
                                        $"Map:\t{mapName}\n" +
                                        $"Entry:\t{entryName} Value:{entryValue}\n" +
                                        $"********************************\n");
                                }
                            }
                        }
                    }
                }
            }
        }
        private void MapSpecialConditions()
        {
            if (Spoiler?.SpecialConditions == null || Maps == null || JsonTracker?.ItemDatabase == null)
                return;

            foreach (Conditions condition in Spoiler.SpecialConditions)
            {
                string? conditionName = condition.Name;
                string? conditionValue = condition.Value;
                string? conditionType = condition.Type;

                foreach (ItemMap itemMap in Maps)
                {
                    string? mapName = itemMap.NoIDItemReference;
                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapType = itemMap.Type;
                    string? mapSpecialType = itemMap.SpecialType;

                    // Match spoiler log entry to map
                    if (!string.Equals(conditionName, mapSpoilerLabel, StringComparison.OrdinalIgnoreCase))
                        continue;

                    foreach (Item item in JsonTracker.ItemDatabase)
                    {
                        string? itemName = item.NoIDItemReference;
                        string? itemType = item.Type;
                        string? itemSpecialType = item.SpecialType;

                        // Ensure this map entry matches this item
                        if (mapSpecialType != itemSpecialType || mapType != itemType || itemName != mapName)
                            continue;

                        if (conditionValue == null || itemMap.Values == null)
                        {
                            Debug.WriteLineIf(DebugStats,
                                $"*** BAD/DUPLICATE MAP VALUES ***\n" +
                                $"Source:\tMapSpecialConditions\n" +
                                $"Item:\t{itemName}\n" +
                                $"Map:\t{mapName}\n" +
                                $"Entry:\t{conditionName} Value:{conditionValue}\n" +
                                $"********************************\n");
                            continue;
                        }

                        itemMap.Values.TryGetValue(conditionValue, out int mappedValue);

                        bool hasChanged = false;
                        string? oldValue = item.OldValue;
                        string? newValue = mappedValue.ToString();

                        switch (itemType)
                        {
                            case "progressive":
                                if (mappedValue != item.StageIndex)
                                {
                                    item.StageIndex = mappedValue;
                                    hasChanged = true;
                                }
                                break;

                            case "toggle":
                                bool newToggleValue = mappedValue <= 0;
                                if (newToggleValue != item.Active)
                                {
                                    item.Active = newToggleValue;
                                    hasChanged = true;
                                }
                                break;

                            case "lua":
                                bool newLuaValue = mappedValue <= 0;
                                if (item.Active != newLuaValue)
                                {
                                    item.Active = newLuaValue;
                                    item.Stage = mappedValue;
                                    hasChanged = true;
                                }
                                break;

                            case "consumable":
                                mappedValue = int.TryParse(conditionValue, out int result)
                                    ? result
                                    : item.AcquiredCount ?? 0;

                                if (mappedValue != item.AcquiredCount)
                                {
                                    item.AcquiredCount = mappedValue;
                                    hasChanged = true;
                                }
                                break;
                        }

                        // Only log if a real change occurred AND the old vs new values differ
                        if (hasChanged && oldValue != newValue)
                        {
                            item.NewValue = newValue;
                            ChangeCount++;
                            ChangeLog += $"Original: {oldValue}\tChange: {newValue}\tEmo: {itemSpecialType}:{itemName}\n";

                            // Sync OldValue to prevent false diffs next time
                            item.OldValue = newValue;
                        }
                    }
                }
            }
        }

        private void MapItems<T>(IEnumerable<T>? source, string sourceType) where T : INameValueCount
        {
            if (source == null || Maps == null || JsonTracker?.ItemDatabase == null)
                return;

            // get Spoiler log item
            foreach (T entry in source)
            {
                string? entryName = entry.Name;
                string? entryValue = entry.Value;
                int? entryCount = entry.Count;

                // get Map
                foreach (ItemMap itemMap in Maps)
                {
                    string? mapName = itemMap.NoIDItemReference;

                    // If map has an ID use it, this is only done for ID relient Items
                    if (itemMap.ID != null)
                    {
                        mapName = itemMap.RawItemReference;
                    }

                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapType = itemMap.Type;

                    // Compare spoilerlog item to map
                    if (string.Equals(entryName, mapSpoilerLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (Item item in JsonTracker.ItemDatabase)
                        {
                            
                            string? itemName = item.NoIDItemReference;
                            string? itemType = item.Type;

                            // If map has an ID Then we will use the full ID itemReference
                            if (itemMap.ID != null)
                            {
                                itemName = item.ItemReference;
                            }
                            // Compare map to item
                            if (mapName == itemName && mapType == itemType)
                            {
                                if (entryValue != null && itemMap.Values != null)
                                {
                                    itemMap.Values.TryGetValue(entryValue, out int mappedValue);

                                    switch (itemType)
                                    {
                                        case "progressive":
                                            if (mappedValue != item.StageIndex)
                                            {
                                                item.StageIndex = mappedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "toggle":
                                            bool newToggleValue = mappedValue <= 0;
                                            if (newToggleValue != item.Active)
                                            {
                                                item.Active = newToggleValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "toggle_badged":
                                            bool newBadgedValue = mappedValue <= 0;
                                            if (newBadgedValue != item.Active)
                                            {
                                                item.Active = newBadgedValue;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                        case "lua":
                                            bool newLuaValue = mappedValue > 0;
                                            if (item.Active != newLuaValue)
                                            {
                                                item.Active = newLuaValue;
                                                item.Stage = mappedValue;
                                                item.NewValue = newLuaValue.ToString() + "," + mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;

                                        case "consumable":
                                            if (entryCount != item.AcquiredCount)
                                            {
                                                item.AcquiredCount = entryCount;
                                                item.NewValue = mappedValue.ToString();
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {itemName}\n";
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    Debug.WriteLineIf(DebugStats,
                                        $"*** BAD/DUPLICATE MAP VALUES ***\n" +
                                        $"Source:\t{sourceType}\n" +
                                        $"Item:\t{itemName}\n" +
                                        $"Map:\t{mapName}\n" +
                                        $"Entry:\t{entryName} Value:{entryValue}\n" +
                                        $"********************************\n");
                                }
                            }
                        }
                    }
                }
            }
        }
        private void MapSharedItems() 
        {
            if (JsonTracker == null || SharedItems == null || Maps == null || JsonTracker.ItemDatabase == null)
                return;

            // Get the sharedItem
            foreach (string sharedItem in SharedItems) 
            { 
                // Get the Map
                foreach (ItemMap itemMap in Maps) 
                {
                    string? mapSpoilerLabel = itemMap.SpoilerLabel;
                    string? mapShared = itemMap.Shared;
                    string? mapItemRef1 = itemMap.RawItemReference;
                    string? mapItemRef2 = itemMap.RawItemReference2;

                    int? onValue1 = itemMap.OnValue;
                    int? onValue2 = itemMap.OnValue2;

                    if (itemMap.Shared == sharedItem) 
                    { 
                        // Get the item from database
                        foreach (Item item in JsonTracker.ItemDatabase) 
                        {
                            string? trackerItemName = item.ItemReference;
                            string? trackerItemType = item.Type;

                            if (trackerItemName == mapItemRef1 || trackerItemName == mapItemRef2) 
                            {
                                // Get the starting item
                                foreach (StartingItem startingItem in Spoiler.StartingItems) 
                                {
                                    string sItemName = startingItem.Name;
                                    string sItemValue = startingItem.Value;

                                    if (mapSpoilerLabel == sItemName) 
                                    {
                                        switch (trackerItemType)
                                        {
                                            case "progressive":
                                                if (trackerItemName == mapItemRef1)
                                                {
                                                    int? newValue1 = onValue1;
                                                    if (newValue1 != item.StageIndex)
                                                    {
                                                        item.StageIndex = onValue1;
                                                        item.NewValue = onValue1.ToString();
                                                        ChangeCount++;
                                                        ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {trackerItemName}\n";
                                                    }
                                                }
                                                else if (trackerItemName == mapItemRef2)
                                                {
                                                    int? newValue2 = onValue2 != null ? onValue2 : onValue1;

                                                    if (newValue2 != item.StageIndex)
                                                    {
                                                        item.StageIndex = newValue2;
                                                        item.NewValue = item.StageIndex.ToString();
                                                        ChangeCount++;
                                                        ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {trackerItemName}\n";
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void MapDungeonRewards()
        {
            if (Spoiler == null || Spoiler.DungeonRewards == null || Maps == null || JsonTracker?.ItemDatabase == null )
                return;

            foreach (DungeonReward dungeonReward in Spoiler.DungeonRewards)
            {
                // Skip if they don't have both values
                if (string.IsNullOrWhiteSpace(dungeonReward.Dungeon) || string.IsNullOrWhiteSpace(dungeonReward.Reward))
                {
                    continue;
                }
              
                foreach (ItemMap itemMap in Maps)
                {
                    string? mapName = itemMap.SpoilerLabel;
                    string? mapRefference = itemMap.RawItemReference;
                    string? mapType = itemMap.MapName;

                    if (mapName == dungeonReward.Dungeon) 
                    {
                        foreach (Item item in JsonTracker.ItemDatabase) 
                        {
                            string? databaseItem = item.NoIDItemReference;
                            string? databaseItemValue = item.OldValue;
                            string? databaseItemType = item.Type;

                            if (mapRefference == "lua:Shadow%20Temple%20Reward" && databaseItem == "lua:Shadow%20Temple%20Reward") 
                            {
                                string test = "a";
                            }

                            if (databaseItem == mapRefference) 
                            {
                                if (itemMap.Values != null && itemMap.OnValue != null && dungeonReward.Value != null)
                                {
                                    itemMap.Values.TryGetValue(dungeonReward.Value, out int mappedValue);

                                    switch (databaseItemType)
                                    {
                                        case "lua":
                                            bool newLuaActive = mappedValue > 0;
                                            if (item.Active != newLuaActive || item.Stage != itemMap.OnValue)
                                            {
                                                item.Active = newLuaActive;
                                                item.Stage = itemMap.OnValue;
                                                item.NewValue = newLuaActive.ToString() + "," + itemMap.OnValue;
                                                ChangeCount++;
                                                ChangeLog += $"Original: {item.OldValue}\tChange: {item.NewValue}\tEmo: {databaseItem}\n";
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #region Exports
        private void DisplayDebugStats(Stopwatch stopWatch)
        {
            stopWatch.Stop();

            Debug.WriteLine(
            $"--- Spoiler to Tracker Converted! ---" +
            $"\nSpoiler:\t\t\t{Spoiler != null}" +
            $"\nMaps:\t\t\t\t{(Maps != null ? Maps.Count : 0)}" +
            $"\nSharedItems:\t\t{SharedItems.Count}" +
            $"\nTracker Items:\t\t{JsonTracker?.ItemDatabase?.Count ?? 0}" +
            $"\nTracker Locations:\t{JsonTracker?.LocationDatabase?.Count ?? 0}" +
            $"\nChanges:\t\t\t{ChangeCount}" +
            $"\nTime Taken:\t\t\t{stopWatch.Elapsed}"
        );
        }
        private async Task ExportTracker(string filePath)
        {
            if (JsonTracker == null)
            {
                Debug.WriteLine("JsonTracker is null, cannot save.");
                return;
            }

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Ignore null properties
                WriteIndented = true,  // Makes the JSON nicely formatted
            };

            try
            {
                using FileStream stream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(stream, JsonTracker, options);
                Debug.WriteLine($"JsonTracker saved successfully to: {filePath}");
                Converted = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save JsonTracker to {filePath}: {ex.Message}");
            }
        }
        #endregion

    }
}
