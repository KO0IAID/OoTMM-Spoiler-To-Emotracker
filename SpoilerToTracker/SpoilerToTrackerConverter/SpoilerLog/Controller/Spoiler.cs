using Microsoft.VisualBasic;
using SpoilerToTrackerConverter.SpoilerLog.Models;
using SpoilerToTrackerConverter.SpoilerLog.Enumerators;
using SpoilerToTrackerConverter.SpoilerLog.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.SpoilerLog.Controller
{
    public class Spoiler
    {
        public string[]? FileContents { get; private set; }
        public bool Parsed { get; private set; }

        #region Collections
        public List<SeedInfo>? SeedInfo { get; private set; } = new();
        public List<Setting>? GameSettings { get; private set; } = new();
        public List<Conditions>? SpecialConditions { get; private set; } = new();
        public List<Trick>? Tricks { get; private set; } = new();
        public List<Glitch>? Glitches { get; private set; } = new();
        public List<StartingItem>? StartingItems { get; private set; } = new();
        public List<JunkLocation>? JunkLocations { get; private set; } = new();
        public List<WorldFlag>? WorldFlags { get; private set; } = new();
        public List<Entrance>? Entrances { get; private set; } = new();
        public List<WayOfTheHeroHint>? WayOfTheHeroHints { get; private set; } = new();
        public List<FoolishHint>? FoolishHints { get; private set; } = new();
        public List<SpecificHint>? SpecificHints { get; private set; } = new();
        public List<RegionalHint>? RegionalHints { get; private set; } = new();
        public List<FoolishRegion>? FoolishRegions { get; private set; } = new();
        public List<WayOfTheHeroPath>? WayOfTheHeroPaths { get; private set; } = new();
        public List<Sphere>? Spheres { get; private set; } = new();
        public List<ItemLocation>? ItemLocations { get; private  set; } = new();

        #endregion
        #region Collections SortBy Enums
        public SortBy SeedInfo_SortBy { get; private set; }
        public SortBy GameSettings_SortBy { get; private set; }
        public SortBy SpecialConditions_SortBy { get; private set; }
        public SortBy Tricks_SortBy { get; private set; }
        public SortBy Glitches_SortBy { get; private set; }
        public SortBy StartingItems_SortBy { get; private set; }
        public SortBy JunkLocations_SortBy { get; private set; }
        public SortBy WorldFlags_SortBy { get; private set; }
        public SortBy Entrances_SortBy { get; private set; }
        public SortBy WayOfTheHeroHints_SortBy { get; private set; }
        public SortBy FoolishHints_SortBy { get; private set; }
        public SortBy SpecificHints_SortBy { get; private set; }
        public SortBy RegionalHints_SortBy { get; private set; }
        public SortBy FoolishRegions_SortBy { get; private set; }
        public SortBy WayOfTheHeroPaths_SortBy { get; private set; }
        public SortBy Spheres_SortBy { get; private set; }
        public SortBy LocationList_SortBy { get; private set; }
        #endregion
        #region Core Functions
        public async Task AddFileContents(string filePath, bool showDebugStats = false)
        {
            Parsed = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Clear();
            FileContents = await File.ReadAllLinesAsync(filePath);

            try
            {
                SeedInfo = await Parse_SeedInfo();
                GameSettings = await Parse_GameSettings();
                SpecialConditions = await Parse_SpecialConditions();
                Tricks = await Parse_Tricks();
                StartingItems = await Parse_StartingItems();
                Glitches = await Parse_Glitches();
                JunkLocations = await Parse_JunkLocations();
                WorldFlags = await Parse_WorldFlags();
                Entrances = await Parse_Entrances();
                WayOfTheHeroHints = await Parse_WayOfTheHeroHints();
                FoolishHints = await Parse_FoolishHints();
                SpecificHints = await Parse_SpecificHints();
                RegionalHints = await Parse_RegionalHints();
                FoolishRegions = await Parse_FoolishRegions();
                WayOfTheHeroPaths = await Parse_WayOfTheHeroPaths();
                Spheres = await Parse_Spheres();
                ItemLocations = await Parse_LocationsList();


                SortCollections();

                if (showDebugStats)
                {
                    DebugStats(sw);
                }

                Parsed = true;

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Exception: {ex}\nMessage: {ex.Message}\nSource: {ex.Source}");
            }

        }
        public void Clear()
        {
            SeedInfo = new();
            GameSettings = new();
            SpecialConditions = new();
            Tricks = new();
            Glitches = new();
            JunkLocations = new();
            WorldFlags = new();
            Entrances = new();
            WayOfTheHeroHints = new();
            FoolishHints = new();
            SpecificHints = new();
            RegionalHints = new();
            FoolishRegions = new();
            WayOfTheHeroPaths = new();
            Spheres = new();
            ItemLocations = new();
            FileContents = null;
        }
        public bool HasValue()
        {
            return FileContents != null && FileContents.Length > 0;
        }
        public async Task<bool> IsSpoilerAsync(string filePath)
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            await using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                bytesRead = fs.Read(buffer, 0, buffer.Length);
            }

            // If it contains control characters other than CR, LF, TAB → likely not plain text
            bool IsASCII =  buffer.Take(bytesRead).All(b =>
                (b == 0x09) || (b == 0x0A) || (b == 0x0D) || (b >= 0x20 && b <= 0x7E) // ASCII text range
            );

            if (!IsASCII)
                return false;

            bool seedExists = false;
            bool settingsStringExist = false;
            bool versionExists = false;
            bool settingsExist = false;

            using var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            for (int i = 0; i < 26 && !reader.EndOfStream; i++)
            {
                string? line = reader.ReadLine();
                if (line != null && line.StartsWith("Seed:"))           { seedExists = true; }
                if (line != null && line.StartsWith("Version:"))        { versionExists = true; }
                if (line != null && line.StartsWith("SettingsString:")) { settingsStringExist = true; }
                if (line != null && line.StartsWith("Settings"))        { settingsExist = true; }

                if (seedExists && settingsStringExist && versionExists && settingsExist)
                    return true;
            }

            return false;
        }
        private void DebugStats(Stopwatch stopWatch)
        {
            stopWatch.Stop();
            Debug.WriteLine(
            $"--- Spoiler Sheet Data Added! ---"  +
            $"\nSeed Info:\t\t\t{(SeedInfo != null ? SeedInfo.Count : 0)}" +
            $"\nGame Settings:\t\t{(GameSettings != null ? GameSettings.Count : 0)}" +
            $"\nSpecial Conditions:\t{(SpecialConditions != null ? SpecialConditions.Count : 0)}" +
            $"\nTricks:\t\t\t\t{(Tricks != null ? Tricks.Count : 0)}" +
            $"\nGlitches:\t\t\t{(Glitches != null ? Glitches.Count : 0)}" +
            $"\nStarting ITems:\t\t{(StartingItems != null ? StartingItems.Count : 0)}" +
            $"\nJunk Locations:\t\t{(JunkLocations != null ? JunkLocations.Count : 0)}" +
            $"\nWorld Flags:\t\t{(WorldFlags != null ? WorldFlags.Count : 0)}" +
            $"\nEntrances:\t\t\t{(Entrances != null ? Entrances.Count : 0)}" +
            $"\nWay Of The Hero:\t{(WayOfTheHeroHints != null ? WayOfTheHeroHints.Count : 0)}" +
            $"\nFoolish Hint:\t\t{(FoolishHints != null ? FoolishHints.Count : 0)}" +
            $"\nSpecific Hint:\t\t{(SpecificHints != null ? SpecificHints.Count : 0)}" +
            $"\nRegional Hint:\t\t{(RegionalHints != null ? RegionalHints.Count : 0)}" +
            $"\nFoolish Regions:\t{(FoolishRegions != null ? FoolishRegions.Count : 0)}" +
            $"\nWayOfTheHero Paths:\t{(WayOfTheHeroPaths != null ? WayOfTheHeroPaths.Count : 0)}" +
            $"\nSpheres:\t\t\t{(Spheres != null ? Spheres.Count : 0)}" +
            $"\nLocations List:\t\t{(ItemLocations != null ? ItemLocations.Count : 0)}"+
            $"\nTime Taken:\t\t\t{ stopWatch.Elapsed}"
            );
            

        }
        public void SortCollections(SortBy sort = SortBy.Default)
        {
            #region GameSettings
            // GameSettings - Alphabetic - (Default)
            if (sort == SortBy.GameSettingsAlphabetic || sort == SortBy.Default)
            {
                if (GameSettings != null)
                {
                    var sortedGameSettings = new List<Setting>(
                    GameSettings.OrderBy(e => e.Name)
                    .ThenBy(e => e.Value)
                    );

                    GameSettings = sortedGameSettings;
                    GameSettings_SortBy = SortBy.GameSettingsAlphabetic;
                }
                
            }

            // GameSettings - Reverse Alphabetic
            if (sort == SortBy.GameSettingsReverseAlphabetic)
            {
                if (GameSettings != null)
                {
                    var sortedGameSettings = new List<Setting>(
                    GameSettings.OrderByDescending(e => e.Name)
                    .ThenBy(e => e.Value)
                    );

                    GameSettings = sortedGameSettings;
                    GameSettings_SortBy = SortBy.GameSettingsReverseAlphabetic;
                }
            }

            // GameSettings - LogOrder
            if (sort == SortBy.GameSettingsLogOrder)
            {
                if (GameSettings != null)
                {
                    var sortedGameSettings = new List<Setting>(
                    GameSettings.OrderBy(e => e.LogOrder)
                    );

                    GameSettings = sortedGameSettings;
                    GameSettings_SortBy = SortBy.GameSettingsLogOrder;
                }
            }
            #endregion
            #region Entrances
            /* Short & Long Defaults */{ 

                // Entrances - Short - (Default)
                if (sort == SortBy.EntrancesShort || sort == SortBy.Default)
                {
                    if (Entrances != null)
                    {
                        var sortedShortEntrances = new List<Entrance>(
                        Entrances.OrderBy(e => e.World)
                       .ThenByDescending(e => e.FromGame)
                       .ThenBy(e => e.ShortEntrance)
                       .ThenBy(e => e.ShortDestination)
                        );

                        Entrances = sortedShortEntrances;
                        Entrances_SortBy = SortBy.EntrancesShort;
                    }

                }
                // Entrances - Long
                if (sort == SortBy.EntrancesLong)
                {
                    if (Entrances != null)
                    {
                        var sortedLongEntrances = new List<Entrance>(
                        Entrances.OrderBy(e => e.World)
                        .ThenByDescending(e => e.FromGame)
                        .ThenBy(e => e.LongEntrance)
                        .ThenBy(e => e.LongDestination)
                        );

                        Entrances = sortedLongEntrances;
                        Entrances_SortBy = SortBy.EntrancesLong;
                    }

                }
            }
            /* Short Alphabetic & Reverse */{

                // Entrances - Short - Alphabetic
                if (sort == SortBy.EntrancesShortAlphabetic)
                {
                    if (Entrances != null)
                    {
                        var sortedLongEntrances = new List<Entrance>(
                        Entrances.OrderBy(e => e.ShortEntrance)
                        .ThenBy(e => e.ShortDestination)
                        .ThenByDescending(e => e.FromGame)
                        .ThenBy(e => e.World)
                        );

                        Entrances = sortedLongEntrances;
                        Entrances_SortBy = SortBy.EntrancesShortAlphabetic;
                    }
                }

                // Entrances - Short - Reverse Alphabetic
                if (sort == SortBy.EntrancesShortReverseAlphabetic)
                {
                    if (Entrances != null)
                    {
                        var sortedLongEntrances = new List<Entrance>(
                        Entrances.OrderByDescending(e => e.ShortEntrance)
                        .ThenByDescending(e => e.ShortDestination)
                        .ThenByDescending(e => e.FromGame)
                        .ThenBy(e => e.World)
                        );

                        Entrances = sortedLongEntrances;
                        Entrances_SortBy = SortBy.EntrancesShortReverseAlphabetic;
                    }

                }
            }
            /* Long  Alphabetic & Reverse */{

                // Entrances - Long - Alphabetic
                if (sort == SortBy.EntrancesLongAlphabetic)
                {
                    if (Entrances != null)
                    {
                        var sortedLongEntrances = new List<Entrance>(
                        Entrances.OrderBy(e => e.LongEntrance)
                        .ThenBy(e => e.LongDestination)
                        .ThenByDescending(e => e.FromGame)
                        .ThenBy(e => e.World)
                        );

                        Entrances = sortedLongEntrances;
                        Entrances_SortBy = SortBy.EntrancesLongAlphabetic;
                    }
                }

                // Entrances - Long - Reverse Alphabetic
                if (sort == SortBy.EntrancesLongReverseAlphabetic)
                {
                    if (Entrances != null)
                    {
                        var sortedLongEntrances = new List<Entrance>(
                        Entrances.OrderByDescending(e => e.LongEntrance)
                        .ThenByDescending(e => e.LongDestination)
                        .ThenByDescending(e => e.FromGame)
                        .ThenBy(e => e.World)
                        );

                        Entrances = sortedLongEntrances;
                        Entrances_SortBy = SortBy.EntrancesLongReverseAlphabetic;
                    }
                }
            }
            /* Short Game & Reverse */{

                // Entrances - Short - Game
                if (sort == SortBy.EntrancesShortGame)
                {
                    if (Entrances != null)
                    {
                        var sortedLongEntrances = new List<Entrance>(
                        Entrances.OrderByDescending(e => e.FromGame)
                        .ThenBy(e => e.ShortEntrance)
                        .ThenBy(e => e.ShortDestination)
                        .ThenBy(e => e.World)
                        );

                        Entrances = sortedLongEntrances;
                        Entrances_SortBy = SortBy.EntrancesShortGame;
                    }
                }

                // Entrances - Long - Game
                if (sort == SortBy.EntrancesLongGame)
                {
                    if (Entrances != null)
                    {
                        var sortedLongEntrances = new List<Entrance>(
                        Entrances.OrderBy(e => e.World)
                        .ThenByDescending(e => e.FromGame)
                        .ThenBy(e => e.LongEntrance)
                        .ThenBy(e => e.LongDestination)
                        );

                        Entrances = sortedLongEntrances;
                        Entrances_SortBy = SortBy.EntrancesLongGame;
                    }
                }

                // Entrances - Short - Reverse Game
                if (sort == SortBy.EntrancesShortReverseGame)
                {
                    if (Entrances != null)
                    {
                        var sortedLongEntrances = new List<Entrance>(
                        Entrances.OrderBy(e => e.FromGame)
                        .ThenBy(e => e.ShortEntrance)
                        .ThenBy(e => e.ShortDestination)
                        .ThenBy(e => e.World)
                        );

                        Entrances = sortedLongEntrances;
                        Entrances_SortBy = SortBy.EntrancesShortReverseGame;
                    }
                }

                // Entrances - Long - Reverse Game
                if (sort == SortBy.EntrancesLongReverseGame)
                {
                    if (Entrances != null)
                    {
                        var sortedLongEntrances = new List<Entrance>(
                        Entrances.OrderBy(e => e.FromGame)
                        .ThenBy(e => e.World)
                        .ThenBy(e => e.LongEntrance)
                        .ThenBy(e => e.LongDestination)
                        );

                        Entrances = sortedLongEntrances;
                        Entrances_SortBy = SortBy.EntrancesLongReverseGame;
                    }
                }
            }
            


           

            #endregion
            #region Tricks
            // Tricks - Alphabetic - (Default)
            if (sort == SortBy.TricksAlphabetic || sort == SortBy.Default)
            {
                if (Tricks != null)
                {
                    var sortedTricks = new List<Trick>(
                    Tricks.OrderBy(e => e.Description)
                    .ThenBy(e => e.Difficulty)
                    .ThenBy(e => e.LogOrder)
                    );

                    Tricks = sortedTricks;
                    Tricks_SortBy = SortBy.TricksAlphabetic;
                }
            }

            // Tricks - Reverse Alphabetic 
            if (sort == SortBy.TricksReverseAlphabetic)
            {
                if (Tricks != null)
                {
                    var sortedTricks = new List<Trick>(
                    Tricks.OrderByDescending(e => e.Description)
                    .ThenBy(e => e.Difficulty)
                    .ThenBy(e => e.LogOrder)
                    );

                    Tricks = sortedTricks;
                    Tricks_SortBy = SortBy.TricksReverseAlphabetic;
                }
            }

            // Tricks - Difficulty
            if (sort == SortBy.TricksDifficulty)
            {
                if(Tricks != null)
                {
                    var sortedTricks = new List<Trick>(
                    Tricks.OrderBy(e => e.Difficulty)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.LogOrder)
                    );

                    Tricks = sortedTricks;
                    Tricks_SortBy= SortBy.TricksDifficulty;
                }
            }

            // Tricks - Log Order
            if (sort == SortBy.TricksLogOrder)
            {
                if (Tricks != null)
                {
                    var sortedTricks = new List<Trick>(
                    Tricks.OrderBy(e => e.LogOrder)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.Difficulty)
                    );

                    Tricks = sortedTricks;
                    Tricks_SortBy = SortBy.TricksLogOrder;
                }
            }
            #endregion
            #region Glitches
            // Glitches - Alphabetic - (Default)
            if (sort == SortBy.GlitchesAlphabetic || sort == SortBy.Default)
            {
                if (Glitches != null)
                {
                    var sortedGlitches = new List<Glitch>(
                    Glitches.OrderBy(e => e.Description)
                    .ThenBy(e => e.Difficulty)
                    .ThenBy(e => e.LogOrder)
                    );

                    Glitches = sortedGlitches;
                    Glitches_SortBy = SortBy.GlitchesAlphabetic;
                }
            }

            // Glitches - Reverse Alphabetic 
            if (sort == SortBy.TricksReverseAlphabetic)
            {
                if (Glitches != null)
                {
                    var sortedGlitches = new List<Glitch>(
                    Glitches.OrderByDescending(e => e.Description)
                    .ThenBy(e => e.Difficulty)
                    .ThenBy(e => e.LogOrder)
                    );

                    Glitches = sortedGlitches;
                    Glitches_SortBy = SortBy.GlitchesReverseAlphabetic;
                }
            }

            // Glitches - Difficulty
            if (sort == SortBy.TricksDifficulty)
            {
                if (Glitches != null)
                {
                    var sortedGlitches = new List<Glitch>(
                    Glitches.OrderBy(e => e.Difficulty)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.LogOrder)
                    );

                    Glitches = sortedGlitches;
                    Glitches_SortBy = SortBy.GlitchesDifficulty;
                }
            }

            // Glitches - Log Order
            if (sort == SortBy.TricksLogOrder)
            {
                if (Glitches != null)
                {
                    var sortedGlitches = new List<Glitch>(
                    Glitches.OrderBy(e => e.LogOrder)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.Difficulty)
                    );

                    Glitches = sortedGlitches;
                    Glitches_SortBy = SortBy.GlitchesLogOrder;
                }
            }
            #endregion
            #region Starting Items
            // Starting Items - Name - (Default)
            if (sort == SortBy.StartingItemsName || sort == SortBy.Default)
            {
                if (StartingItems != null)
                {
                    var sortedStartingItems = new List<StartingItem>(
                    StartingItems.OrderBy(e => e.Name)
                    .ThenBy(e => e.Count)
                    .ThenBy(e => e.Value)
                    );

                    StartingItems = sortedStartingItems;
                    StartingItems_SortBy = SortBy.FoolishRegionsWorld;
                }
            }

            // Starting Items - Value
            if (sort == SortBy.StartingItemsValue)
            {
                if (StartingItems != null)
                {
                    var sortedStartingItems = new List<StartingItem>(
                    StartingItems.OrderBy(e => e.Value)
                    .ThenBy(e => e.Name)
                    .ThenBy(e => e.Count)
                    );

                    StartingItems = sortedStartingItems;
                    StartingItems_SortBy = SortBy.FoolishRegionsRegion;
                }
            }

            // Starting Items - Count
            if (sort == SortBy.StartingItemsCount)
            {
                if (StartingItems != null)
                {
                    var sortedStartingItems = new List<StartingItem>(
                    StartingItems.OrderBy(e => e.Count)
                    .ThenBy(e => e.Name)
                    .ThenBy(e => e.Value)
                    );

                    StartingItems = sortedStartingItems;
                    StartingItems_SortBy = SortBy.FoolishRegionsCount;
                }
            }
            #endregion
            // Junk Locations Missing
            // World Flags Missing
            #region WayOfTheHero Hints

            // WayOfTheHero Hints- World - (Default)
            if (sort == SortBy.WayOfTheHeroHintsWorld || sort == SortBy.Default)
            {
                if (WayOfTheHeroHints != null)
                {
                    var sortedWayOfTheHeroHints = new List<WayOfTheHeroHint>(
                    WayOfTheHeroHints.OrderBy(e => e.World)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.Location)
                    );

                    WayOfTheHeroHints = sortedWayOfTheHeroHints;
                    WayOfTheHeroHints_SortBy = SortBy.WayOfTheHeroHintsWorld;
                }
            }

            // WayOfTheHero Hints - Location
            if (sort == SortBy.WayOfTheHeroHintsLocation)
            {
                if (WayOfTheHeroHints != null)
                {
                    var sortedWayOfTheHeroHints = new List<WayOfTheHeroHint>(
                    WayOfTheHeroHints.OrderBy(e => e.Location)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Item)
                    );

                    WayOfTheHeroHints = sortedWayOfTheHeroHints;
                    WayOfTheHeroHints_SortBy = SortBy.WayOfTheHeroHintsLocation;
                }
            }

            // WayOfTheHero Hints - Items
            if (sort == SortBy.WayOfTheHeroHintsItems)
            {
                if (WayOfTheHeroHints != null)
                {
                    var sortedWayOfTheHeroHints = new List<WayOfTheHeroHint>(
                    WayOfTheHeroHints.OrderBy(e => e.Item)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.Location)
                    );

                    WayOfTheHeroHints = sortedWayOfTheHeroHints;
                    WayOfTheHeroHints_SortBy = SortBy.WayOfTheHeroHintsItems;
                }
            }


            #endregion 
            #region Foolish Hints
            // FoolishHints - World - (Default)
            if (sort == SortBy.FoolishHintsWorld || sort == SortBy.Default)
            {
                if (FoolishHints != null)
                {
                    var sortedFoolishHints = new List<FoolishHint>(
                    FoolishHints.OrderBy(e => e.World)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.Location)
                    );

                    FoolishHints = sortedFoolishHints;
                    FoolishHints_SortBy = SortBy.FoolishHintsWorld;
                }
            }

            // WayOfTheHero - Gossip
            if (sort == SortBy.FoolishHintsGossip)
            {
                if (FoolishHints != null)
                {
                    var sortedFoolishHints = new List<FoolishHint>(
                    FoolishHints.OrderBy(e => e.GossipStone)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Location)
                    );

                    FoolishHints = sortedFoolishHints;
                    FoolishHints_SortBy = SortBy.FoolishHintsGossip;
                }
            }

            // WayOfTheHero - Location
            if (sort == SortBy.FoolishHintsLocation)
            {
                if (FoolishHints != null)
                {
                    var sortedFoolishHints = new List<FoolishHint>(
                    FoolishHints.OrderBy(e => e.Location)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.World)
                    );

                    FoolishHints = sortedFoolishHints;
                    FoolishHints_SortBy = SortBy.FoolishHintsLocation;
                }
            }


            #endregion
            #region Specific Hints
            // SpecificHints - World - (Default)
            if (sort == SortBy.SpecificHintsWorld || sort == SortBy.Default)
            {
                if (SpecificHints != null)
                {
                    var sortedSpecificHints = new List<SpecificHint>(
                    SpecificHints.OrderBy(e => e.World)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.Location)
                    .ThenBy(e => e.Item)
                    );

                    SpecificHints = sortedSpecificHints;
                    FoolishHints_SortBy = SortBy.SpecificHintsWorld;
                }
            }

            // SpecificHints - Gossip
            if (sort == SortBy.SpecificHintsGossip)
            {
                if (SpecificHints != null)
                {
                    var sortedSpecificHints = new List<SpecificHint>(
                    SpecificHints.OrderBy(e => e.GossipStone)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Location)
                    .ThenBy(e => e.Item)
                    );

                    SpecificHints = sortedSpecificHints;
                    SpecificHints_SortBy = SortBy.SpecificHintsGossip;
                }
            }

            // SpecificHints - Location
            if (sort == SortBy.SpecificHintsLocation)
            {
                if (SpecificHints != null)
                {
                    var sortedSpecificHints = new List<SpecificHint>(
                    SpecificHints.OrderBy(e => e.Location)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Item)
                    );

                    SpecificHints = sortedSpecificHints;
                    SpecificHints_SortBy = SortBy.SpecificHintsLocation;
                }
            }

            // SpecificHints - Itempoly
            if (sort == SortBy.SpecificHintsItem)
            {
                if (SpecificHints != null)
                {
                    var sortedSpecificHints = new List<SpecificHint>(
                    SpecificHints.OrderBy(e => e.Item)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Location)
                    );

                    SpecificHints = sortedSpecificHints;
                    SpecificHints_SortBy = SortBy.SpecificHintsItem;
                }
            }

            #endregion
            #region Regional Hints
            // RegionalHints - World - (Default)
            if (sort == SortBy.RegionalHintsWorld || sort == SortBy.Default)
            {
                if (RegionalHints != null)
                {
                    var sortedRegionalHints = new List<RegionalHint>(
                    RegionalHints.OrderBy(e => e.World)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.Region)
                    .ThenBy(e => e.Item)
                    );

                    RegionalHints = sortedRegionalHints;
                    RegionalHints_SortBy = SortBy.RegionalHintsWorld;
                }
            }

            // RegionalHints - Gossip
            if (sort == SortBy.RegionalHintsGossip)
            {
                if (RegionalHints != null)
                {
                    var sortedRegionalHints = new List<RegionalHint>(
                    RegionalHints.OrderBy(e => e.GossipStone)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Region)
                    .ThenBy(e => e.Item)
                    );

                    RegionalHints = sortedRegionalHints;
                    RegionalHints_SortBy = SortBy.RegionalHintsGossip;
                }
            }

            // RegionalHints - Region
            if (sort == SortBy.RegionalHintsRegion)
            {
                if (RegionalHints != null)
                {
                    var sortedRegionalHints = new List<RegionalHint>(
                    RegionalHints.OrderBy(e => e.Region)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Item)
                    );

                    RegionalHints = sortedRegionalHints;
                    RegionalHints_SortBy = SortBy.RegionalHintsRegion;
                }
            }

            // RegionalHints - Itempoly
            if (sort == SortBy.RegionalHintsItem)
            {
                if (RegionalHints != null)
                {
                    var sortedRegionalHints = new List<RegionalHint>(
                    RegionalHints.OrderBy(e => e.Item)
                    .ThenBy(e => e.GossipStone)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Region)
                    );

                    RegionalHints = sortedRegionalHints;
                    RegionalHints_SortBy = SortBy.RegionalHintsItem;
                }
            }

            #endregion
            #region Foolish Regions
            // FoolishRegions - World - (Default)
            if (sort == SortBy.FoolishRegionsWorld || sort == SortBy.Default)
            {
                if (FoolishRegions != null)
                {
                    var sortedFoolishRegions = new List<FoolishRegion>(
                    FoolishRegions.OrderBy(e => e.World)
                    .ThenBy(e => e.Count)
                    .ThenBy(e => e.Region)
                    );

                    FoolishRegions = sortedFoolishRegions;
                    FoolishRegions_SortBy = SortBy.FoolishRegionsWorld;
                }
            }

            // FoolishRegions - Region
            if (sort == SortBy.FoolishRegionsRegion)
            {
                if (FoolishRegions != null)
                {
                    var sortedFoolishRegions = new List<FoolishRegion>(
                    FoolishRegions.OrderBy(e => e.Region)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Count)
                    );

                    FoolishRegions = sortedFoolishRegions;
                    FoolishRegions_SortBy = SortBy.FoolishRegionsRegion;
                }
            }

            // FoolishRegions - Count
            if (sort == SortBy.FoolishRegionsCount)
            {
                if (FoolishRegions != null)
                {
                    var sortedFoolishRegions = new List<FoolishRegion>(
                    FoolishRegions.OrderBy(e => e.Count)
                    .ThenBy(e => e.Region)
                    .ThenBy(e => e.World)
                    );

                    FoolishRegions = sortedFoolishRegions;
                    FoolishRegions_SortBy = SortBy.FoolishRegionsCount;
                }
            }
            #endregion
            #region WayOfTheHero Paths
            // WayOfTheHero Paths - World - (Default)
            if (sort == SortBy.WayOfTheHeroPathsLogOrder || sort == SortBy.Default)
            {
                if (WayOfTheHeroPaths != null)
                {
                    var sortedWayOfTheHeroPaths = new List<WayOfTheHeroPath>(
                    WayOfTheHeroPaths.OrderBy(e => e.World)
                    .ThenBy(e=> e.LogOrder)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    );

                    WayOfTheHeroPaths = sortedWayOfTheHeroPaths;
                    WayOfTheHeroPaths_SortBy = SortBy.WayOfTheHeroPathsWorld;
                }
            }
            if (sort == SortBy.WayOfTheHeroPathsWorld || sort == SortBy.WayOfTheHeroPathsWorld)
            {
                if (WayOfTheHeroPaths != null)
                {
                    var sortedWayOfTheHeroPaths = new List<WayOfTheHeroPath>(
                    WayOfTheHeroPaths.OrderBy(e => e.World)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    );

                    WayOfTheHeroPaths = sortedWayOfTheHeroPaths;
                    WayOfTheHeroPaths_SortBy = SortBy.WayOfTheHeroPathsWorld;
                }
            }

            // WayOfTheHero Paths - Description
            if (sort == SortBy.WayOfTheHeroPathsDescription)
            {
                if (WayOfTheHeroPaths != null)
                {
                    var sortedWayOfTheHeroPaths = new List<WayOfTheHeroPath>(
                    WayOfTheHeroPaths.OrderBy(e => e.Description)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    );

                    WayOfTheHeroPaths = sortedWayOfTheHeroPaths;
                    WayOfTheHeroPaths_SortBy = SortBy.WayOfTheHeroPathsDescription;
                }
            }

            // WayOfTheHero Paths - Player
            if (sort == SortBy.WayOfTheHeroPathsPlayer)
            {
                if (WayOfTheHeroPaths != null)
                {
                    var sortedWayOfTheHeroPaths = new List<WayOfTheHeroPath>(
                    WayOfTheHeroPaths.OrderBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.World)
                    );

                    WayOfTheHeroPaths = sortedWayOfTheHeroPaths;
                    WayOfTheHeroPaths_SortBy = SortBy.WayOfTheHeroPathsPlayer;
                }
            }
            // WayOfTheHero Paths - Itempoly
            if (sort == SortBy.WayOfTheHeroPathsItem)
            {
                if (WayOfTheHeroPaths != null)
                {
                    var sortedWayOfTheHeroPaths = new List<WayOfTheHeroPath>(
                    WayOfTheHeroPaths.OrderBy(e => e.Item)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.World)
                    );

                    WayOfTheHeroPaths = sortedWayOfTheHeroPaths;
                    WayOfTheHeroPaths_SortBy = SortBy.WayOfTheHeroPathsItem;
                }
            }
            #endregion
            #region Spheres
            // Spheres - World - (Default)
            if (sort == SortBy.SpheresWorld || sort == SortBy.Default)
            {
                if (Spheres != null)
                {
                    var sortedSpheres = new List<Sphere>(
                    Spheres.OrderBy(e => e.World)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Type)
                    .ThenBy(e => e.Location)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    );

                    Spheres = sortedSpheres;
                    Spheres_SortBy = SortBy.SpheresWorld;
                }
            }

            // Spheres - Type
            if (sort == SortBy.SpheresType)
            {
                if (Spheres != null)
                {
                    var sortedSpheres = new List<Sphere>(
                    Spheres.OrderBy(e => e.Type)
                    .ThenBy(e => e.Location)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Item)
                    );

                    Spheres = sortedSpheres;
                    Spheres_SortBy = SortBy.SpheresType;
                }
            }

            // Spheres - Number
            if (sort == SortBy.SpheresNumber)
            {
                if (Spheres != null)
                {
                    var sortedSpheres = new List<Sphere>(
                    Spheres.OrderBy(e => e.Number)
                    .ThenBy(e => e.Type)
                    .ThenBy(e => e.Location)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Item)
                    );

                    Spheres = sortedSpheres;
                    Spheres_SortBy = SortBy.SpheresNumber;
                }
            }
            // Spheres - Location
            if (sort == SortBy.SpheresLocation)
            {
                if (Spheres != null)
                {
                    var sortedSpheres = new List<Sphere>(
                    Spheres.OrderBy(e => e.Location)
                    .ThenBy(e => e.Type)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Item)
                    );

                    Spheres = sortedSpheres;
                    Spheres_SortBy = SortBy.SpheresLocation;
                }
            }
            // Spheres - Player
            if (sort == SortBy.SpheresPlayer)
            {
                if (Spheres != null)
                {
                    var sortedSpheres = new List<Sphere>(
                    Spheres.OrderBy(e => e.Player)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Type)
                    .ThenBy(e => e.Location)
                    .ThenBy(e => e.Item)
                    );

                    Spheres = sortedSpheres;
                    Spheres_SortBy = SortBy.SpheresPlayer;
                }
            }
            // Spheres - Itempoly
            if (sort == SortBy.SpheresItem)
            {
                if (Spheres != null)
                {
                    var sortedSpheres = new List<Sphere>(
                    Spheres.OrderBy(e => e.Item)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Type)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Location)
                    );

                    Spheres = sortedSpheres;
                    Spheres_SortBy = SortBy.SpheresItem;
                }
            }
            #endregion
            #region Locations List
            // Locations List - World - (Default)
            if (sort == SortBy.LocationsListWorld || sort == SortBy.Default)
            {
                if (ItemLocations != null)
                {
                    var sortedLocationsList = new List<ItemLocation>(
                    ItemLocations.OrderBy(e => e.World)
                    .ThenByDescending(e => e.Game)
                    .ThenBy(e => e.Count)
                    .ThenBy(e => e.Region)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.Item)
                    );

                    ItemLocations = sortedLocationsList;
                    LocationList_SortBy = SortBy.LocationsListWorld;
                }
            }

            // Locations List - Game
            if (sort == SortBy.LocationsListGame)
            {
                if (ItemLocations != null)
                {
                    var sortedLocationsList = new List<ItemLocation>(
                    ItemLocations.OrderByDescending(e => e.Game)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Count)
                    .ThenBy(e => e.Region)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    );

                    ItemLocations = sortedLocationsList;
                    LocationList_SortBy = SortBy.LocationsListGame;
                }
            }

            // Locations List - Region
            if (sort == SortBy.LocationsListRegion)
            {
                if (ItemLocations != null)
                {
                    var sortedLocationsList = new List<ItemLocation>(
                    ItemLocations.OrderBy(e => e.Region)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.Count)
                    .ThenBy(e => e.World)
                    .ThenByDescending(e => e.Game)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    );

                    ItemLocations = sortedLocationsList;
                    LocationList_SortBy = SortBy.LocationsListRegion;
                }
            }
            // Locations List - Number
            if (sort == SortBy.LocationsListNumber)
            {
                if (ItemLocations != null)
                {
                    var sortedLocationsList = new List<ItemLocation>(
                    ItemLocations.OrderBy(e => e.Number)
                    .ThenBy(e => e.Count)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.World)
                    .ThenByDescending(e => e.Game)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    );

                    ItemLocations = sortedLocationsList;
                    LocationList_SortBy = SortBy.LocationsListNumber;
                }
            }
            // Locations List - Count
            if (sort == SortBy.LocationsListCount)
            {
                if (ItemLocations != null)
                {
                    var sortedLocationsList = new List<ItemLocation>(
                    ItemLocations.OrderBy(e => e.Count)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.World)
                    .ThenByDescending(e => e.Game)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    );

                    ItemLocations = sortedLocationsList;
                    LocationList_SortBy = SortBy.LocationsListCount;
                }
            }
            // Locations List - Description
            if (sort == SortBy.LocationsListDescription)
            {
                if (ItemLocations != null)
                {
                    var sortedLocationsList = new List<ItemLocation>(
                    ItemLocations.OrderBy(e => e.Description)
                    .ThenBy(e => e.Region)
                    .ThenBy(e => e.Count)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.World)
                    .ThenByDescending(e => e.Game)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Item)
                    );

                    ItemLocations = sortedLocationsList;
                    LocationList_SortBy = SortBy.LocationsListDescription;
                }
            }
            // Locations List - Player
            if (sort == SortBy.LocationsListPlayer)
            {
                if (ItemLocations != null)
                {
                    var sortedLocationsList = new List<ItemLocation>(
                    ItemLocations.OrderBy(e => e.Player)
                    .ThenByDescending(e => e.Game)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Count)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Region)
                    .ThenBy(e => e.Description)
                    .ThenBy(e => e.Item)
                    );

                    ItemLocations = sortedLocationsList;
                    LocationList_SortBy = SortBy.LocationsListPlayer;
                }
            }
            // Locations List - Itempoly
            if (sort == SortBy.LocationsListItem)
            {
                if (ItemLocations != null)
                {
                    var sortedLocationsList = new List<ItemLocation>(
                    ItemLocations.OrderBy(e => e.Item)
                    .ThenBy(e => e.Player)
                    .ThenBy(e => e.Count)
                    .ThenBy(e => e.Number)
                    .ThenBy(e => e.Region)
                    .ThenByDescending(e => e.Game)
                    .ThenBy(e => e.World)
                    .ThenBy(e => e.Description)
                    );

                    ItemLocations = sortedLocationsList;
                    LocationList_SortBy = SortBy.LocationsListItem;
                }
            }
            #endregion
        }
        #endregion
        #region Data Parsing Helper Methods
        private async Task<List<T>?> AddValues<T>(Tuple<int, int> range, string[] fileContents) where T : ICreateFromLine<T>, new()
        {
            return await Task.Run(() =>
            {
                // Validate
                if (range == null || fileContents == null) return null;
                if (range.Item1 < 0 || range.Item2 < 0) return null;

                int start = Math.Max(0, range.Item1);
                int end = Math.Min(range.Item2, fileContents.Length - 1);

                // If start > end => nothing to add, return empty list (not null)
                if (start > end) return new List<T>();

                var collection = new List<T>();
                var parser = new T();

                // IMPORTANT: iterate INCLUSIVE of 'end'
                for (int i = start; i <= end; i++)
                {
                    string line = fileContents[i];

                    // skip blank lines (optional, but safer)
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // If your CreateFromLine expects un-indented lines, trim leading spaces:
                    string parsedLine = line.TrimStart();

                    T item = parser.CreateFromLine(parsedLine);
                    collection.Add(item);
                }

                return collection;
            });
        }
        private async Task<KeyValuePair<string, string>?> Parse_SingleKeyValue(string[] file, string categoryName, string delimiter = ":", int startingPosition = 0)
        {
            return await Task.Run(() =>
            {
                int position = startingPosition;

                while (position < file.Length &&
                       !file[position].TrimStart().StartsWith(categoryName + delimiter, StringComparison.OrdinalIgnoreCase))
                {
                    position++;
                }

                if (position >= file.Length)
                    return (KeyValuePair<string, string>?)null;

                string[] line = file[position].Split(new[] { delimiter }, 2, StringSplitOptions.None);

                if (line.Length < 2)
                    return (KeyValuePair<string, string>?)null;

                return new KeyValuePair<string, string>(line[0].Trim(), line[1].Trim());
            });
        }
        private async Task<List<JunkLocation>> Parse_Junk(string categoryName, string[] file, int startingPosition = 0)
        {
            return await Task.Run(() =>
            {
                int position = startingPosition;
                List<JunkLocation> list = new();

                // Find the category header
                while (position < file.Length && !file[position].TrimStart().StartsWith(categoryName, StringComparison.OrdinalIgnoreCase))
                {
                    position++;
                }

                // If the category header is not found, return empty list
                if (position >= file.Length)
                    return list;

                // Move past the category header to the first data line
                position++;

                // Parse strings until a blank line or un-indented line is found
                while (position < file.Length)
                {
                    string currentLine = file[position];
                    string trimmedLine = currentLine.Trim();

                    if (string.IsNullOrEmpty(trimmedLine) || !currentLine.StartsWith("  "))
                        break;

                    // Create a JunkLocation object instead of adding a raw string
                    list.Add(new JunkLocation { Location = trimmedLine });

                    position++;
                }

                return list;
            });
        }

        private async Task<Tuple<int, int>> FindCategoryRange(string categoryName, string[] file, int startingPosition = 0)
        {
            return await Task.Run(() =>
            {
                int position = startingPosition;
                int start = -1;
                int end = file.Length - 1;

                // Search for Category Header
                while (position < file.Length && !file[position].Trim().Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                    position++;

                // Category not found
                if (position >= file.Length)
                {
                    return Tuple.Create(-1, -1);
                }

                // Category Found: 
                // Move past the category header
                position++;
                start = position;

                while (position < file.Length)
                {
                    string line = file[position];

                    // Still In Category
                    if (line.StartsWith("  "))
                    {
                        position++;
                    }
                    // Possible End of Category
                    else if (string.IsNullOrWhiteSpace(line))
                    {
                        // Peek ahead && Still indented
                        if (position + 1 < file.Length && file[position + 1].StartsWith("  "))
                        {
                            position += 2;
                        }
                        // End of Category Reached
                        else
                        {
                            break;
                        }
                    }
                    // Safety Net
                    else
                    {
                        break;
                    }
                }
                end = position - 1;
                return Tuple.Create(start, end);
            });
        }
        private async Task<BlockInfo?> FindBlock_Generic(string[] file, string? header = null, string? subHeader = null, int startingPosition = 0, bool multiWorld = false)
        {
            return await Task.Run(() =>
            {
                int position = startingPosition;

                // Step 1: Find the main header
                while (header != null && position < file.Length &&
                       !file[position].Trim().Equals(header, StringComparison.OrdinalIgnoreCase))
                {
                    position++;
                }

                // Header not found
                if (position >= file.Length)
                    return null;

                // Header found
                int headerPosition = position;
                int startLine = headerPosition + 1;
                int endLine = startLine;

                bool subHeaderFound = false;
                bool worldHeaderFound = false;

                string? world = null;
                Regex worldRegex = new Regex(@"^  World \d+:?$");

                string indent = "  "; // base indent after header (2 spaces)

                // Step 2: Start parsing lines under the header
                while (endLine < file.Length)
                {
                    string line = file[endLine];

                    // Match multiworld "  World x:" header
                    if (multiWorld && !worldHeaderFound && worldRegex.IsMatch(line))
                    {
                        world = line.Trim().TrimEnd(':');
                        worldHeaderFound = true;
                        startLine = endLine + 1;
                        indent += "  ";
                    }

                    // Match subheader
                    if (subHeader != null && !subHeaderFound && line.StartsWith(indent) &&
                        line.IndexOf(subHeader, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        subHeader = line.Trim();
                        subHeaderFound = true;
                        startLine = endLine + 1;
                        indent += "  ";
                    }

                    // Exit block if we hit a line that is blank
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        endLine++;
                        break;
                    }

                    endLine++;
                }

                // If subheader was requested but not found, return null
                if (subHeader != null && !subHeaderFound)
                    return null;

                // No actual content in block
                if (startLine > endLine - 1)
                    return null;

                return new BlockInfo
                {
                    Header = header,
                    SubHeader = subHeader,
                    StartLine = startLine,
                    EndLine = endLine - 1,
                    World = world,
                    MultiWorld = worldHeaderFound,
                    HasValue = true
                };
            });

        }
        private async Task<BlockInfo?> FindBlock_SpecialConditions(string header, string[] file, int startingPosition = 0)
        {
            return await Task.Run(() =>
            {
                int position = startingPosition;

                // Finds the header
                while (position < file.Length && !file[position].Trim().Equals(header, StringComparison.OrdinalIgnoreCase))
                    position++;

                if (position >= file.Length)
                    return null;

                // Header has been found: Get the range
                int startLine = position + 1;
                int endLine = startLine;
                string? subHeader = null;

                while (endLine < file.Length)
                {
                    string line = file[endLine];

                    // Detect first subheader (e.g., "  BRIDGE:")
                    if (subHeader == null && line.StartsWith("  ") && line.Trim().EndsWith(":") && !line.Contains(" "))
                    {
                        subHeader = line.Trim().TrimEnd(':');
                    }

                    // Exit block if we hit a new top-level section
                    if (!line.StartsWith("  ") && !string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }

                    endLine++;
                }

                return new BlockInfo
                {
                    Header = header,
                    SubHeader = subHeader,
                    StartLine = startLine,
                    EndLine = endLine - 1

                };
            });
            
        }
        private async Task<BlockInfo?> FindBlock_Hint(string sectionName, string[] fileContents, int worldNum = 1, int? startLine = null)
        {
            return await Task.Run(() =>
            {
                if (fileContents == null || fileContents.Length == 0)
                    return null;

                BlockInfo block = new BlockInfo();
                bool inHintsSection = false;
                bool foundHeader = false;
                int headerPos = -1;

                // Go through file lines starting from startLine (or 0 if null)
                for (int i = startLine ?? 0; i < fileContents.Length; i++)
                {
                    string line = fileContents[i].Trim();

                    // --- Detect top-level sections
                    if (Regex.IsMatch(line, @"^(Seed Info|Game Settings|Special Conditions|Tricks|Starting Items|Glitches|Junk Locations|World Flags|Entrances|Hints|Paths|Spheres|Locations?)\b", RegexOptions.IgnoreCase))
                    {
                        // Entering Hints section
                        if (Regex.IsMatch(line, @"^Hints\b", RegexOptions.IgnoreCase))
                        {
                            inHintsSection = true;
                            continue;
                        }
                        // Leaving Hints section (hit another top-level section)
                        else if (inHintsSection)
                        {
                            // We've exited the Hints section; stop searching
                            break;
                        }
                    }

                    // --- Look for the target header inside Hints only
                    if (inHintsSection && Regex.IsMatch(line, @$"^{Regex.Escape(sectionName)}", RegexOptions.IgnoreCase))
                    {
                        foundHeader = true;
                        headerPos = i;
                        block.HeaderPosition = i;
                        block.StartLine = i + 1;
                        block.World = $"World {worldNum}";
                        continue;
                    }

                    // --- Detect end of block
                    if (foundHeader && (string.IsNullOrWhiteSpace(line) ||
                        Regex.IsMatch(line, @"^(Seed Info|Game Settings|Special Conditions|Tricks|Starting Items|Glitches|Junk Locations|World Flags|Entrances|Hints|Paths|Spheres|Locations?)\b", RegexOptions.IgnoreCase)))
                    {
                        block.EndLine = i - 1;
                        break;
                    }
                }

                // No valid header found
                if (!foundHeader)
                    return null;

                // --- Detect multiworld section (optional)
                block.MultiWorld = fileContents
                    .Skip(block.StartLine)
                    .Any(l => l.TrimStart().StartsWith("World ", StringComparison.OrdinalIgnoreCase));

                return block;
            });
        }
        private async Task<BlockInfo?> FindBlock_LocationsList(string[] file, string? header = null, int startingPosition = 0, bool multiWorld = false, string? world = null, int? headerPosition = null)
        {
            return await Task.Run(() =>
            {
                int position = startingPosition;

                // Step 1: Find the main header (if provided)
                if (header != null)
                {
                    while (position < file.Length &&
                           !file[position].Trim().Contains(header, StringComparison.OrdinalIgnoreCase))
                    {
                        position++;
                    }

                    // Header not found
                    if (position >= file.Length)
                        return null;
                }

                // If not already set, set the header position
                if (headerPosition == null)
                    headerPosition = position;

                int startLine = position;
                int endLine = startLine + 1;

                string? subHeader = null;
                bool subHeaderFound = false;
                bool worldHeaderFound = false;

                Regex worldRegex = new Regex(@"^  World\s\d");

                string indent = "  "; // base indent after header (2 spaces)

                // Step 2: Start parsing lines under the header
                while (endLine < file.Length)
                {
                    string line = file[endLine];

                    // Match multiworld "  World x" header
                    if (multiWorld && !worldHeaderFound && worldRegex.IsMatch(line))
                    {
                        string[] parts = Regex.Split(line, @"\s*\((\d+)\)\s*$");
                        if (parts.Length >= 1)
                        {
                            world = parts[0].Trim();
                            worldHeaderFound = true;
                            startLine = endLine + 1;
                            indent += "  ";
                        }
                    }

                    // Find subheader
                    if (!subHeaderFound && line.StartsWith(indent) && line.Trim().EndsWith(':'))
                    {
                        subHeader = line.Trim();
                        subHeaderFound = true;
                        startLine = endLine + 1;
                        indent += "  ";
                    }

                    // Exit block if we hit a blank line (end of this block)
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        endLine++;
                        break;
                    }

                    endLine++;
                }

                // If a subheader was expected but not found, return null
                if (subHeader != null && !subHeaderFound)
                    return null;

                if (startLine >= endLine || startLine >= file.Length - 1)
                    return null;

                return new BlockInfo
                {
                    Header = header,
                    HeaderPosition = headerPosition,
                    SubHeader = subHeader,
                    StartLine = startLine,
                    EndLine = endLine - 1,
                    World = world,
                    MultiWorld = worldHeaderFound,
                    HasValue = true,
                    FileLength = file.Length,
                };
            });

        }
        #endregion
        #region Data Parsing
        private async Task<List<SeedInfo>?> Parse_SeedInfo()
        {

            if (FileContents == null)
            {
                return null;
            }


            var seedInfo = new List<SeedInfo>();

            SeedInfo seed = new SeedInfo { Pair = await Parse_SingleKeyValue(FileContents, "Seed") };
            SeedInfo version = new SeedInfo { Pair = await Parse_SingleKeyValue(FileContents, "Version") };
            SeedInfo settings = new SeedInfo { Pair = await Parse_SingleKeyValue(FileContents, "SettingsString") };

            if (seed.Pair.HasValue) seedInfo.Add(seed);
            if (version.Pair.HasValue) seedInfo.Add(version);
            if (settings.Pair.HasValue) seedInfo.Add(settings);

            return seedInfo;


        }
        private async Task<List<Setting>?> Parse_GameSettings()
        {

            if (FileContents == null)
            {
                return null;
            }

            var range = await FindCategoryRange("Settings", FileContents);

            var settings = await AddValues<Setting>(range, FileContents);

            return settings;
        }
        private async Task<List<Conditions>?> Parse_SpecialConditions()
        {

            if (FileContents == null)
            {
                return null;
            }

            var result = new List<Conditions>();
            var block = await FindBlock_SpecialConditions("Special Conditions", FileContents);

            if (block == null) return result;

            string? currentType = null;

            for (int i = block.StartLine; i <= block.EndLine; i++)
            {
                string line = FileContents[i].Trim();

                if (string.IsNullOrWhiteSpace(line)) continue;

                // Detect new subheader (e.g., BRIDGE:)
                if (!line.StartsWith("-") && line.EndsWith(":") && !line.Contains(" "))
                {
                    currentType = line.Replace(":", "").Trim();
                    continue;
                }

                // Parse key-value under current type
                var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    string key = parts[0];
                    string value = parts[1];

                    if (key.Equals("count", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out int count))
                        result.Add(new Conditions(currentType, key, value, count));
                    else
                        result.Add(new Conditions(currentType, key, value));
                }
            }

            return result;
        }
        private async Task<List<Trick>?> Parse_Tricks()
        {
            if (FileContents == null)
                return null;

            var range = await FindCategoryRange("Tricks", FileContents);

            var tricks = await AddValues<Trick>(range, FileContents);

            return tricks;
        }
        private async Task<List<Glitch>?> Parse_Glitches()
        {

            if (FileContents == null)
            {
                return null;
            }

            var range = await FindCategoryRange("Glitches", FileContents);

            var glitches = await AddValues<Glitch>(range, FileContents);

            return glitches;
        }
        private async Task<List<StartingItem>?> Parse_StartingItems() 
        {
            if (FileContents == null)
                return null;

            var range = await FindCategoryRange("Starting Items", FileContents);

            var startingItems = await AddValues<StartingItem>(range, FileContents);

            return startingItems;

        }
        private async Task<List<JunkLocation>?> Parse_JunkLocations()
        {
            if (FileContents == null)
            {
                return null;
            }

            return await Parse_Junk("Junk Locations", FileContents);
        }
        private async Task<List<WorldFlag>?> Parse_WorldFlags()
        {

            if (FileContents == null)
            {
                return null;
            }

            var worldFlags = new List<WorldFlag>();

            var (start, end) = await FindCategoryRange("World Flags", FileContents);
            if (start == -1) return worldFlags;

            string? currentWorld = null;

            for (int i = start; i <= end; i++)
            {
                string line = FileContents[i];

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Detect World header for multiplayer
                if (!line.StartsWith("    ") && line.Trim().StartsWith("World", StringComparison.OrdinalIgnoreCase))
                {
                    currentWorld = line.Trim();
                    continue;
                }
                // Detect if its singleplayer
                else if (line.StartsWith("  "))
                {
                    currentWorld = "";
                }

                // Parse Multiplayer indented flag lines (e.g., "    Ganon Trials: none")
                if (line.StartsWith("    ") && currentWorld != null)
                {
                    var parts = line.Trim().Split(':', 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var condition = parts[0].Trim();
                        var value = parts[1].Trim();

                        worldFlags.Add(new WorldFlag
                        {
                            World = currentWorld,
                            Condition = condition,
                            Value = value
                        });
                    }
                }
                // Parse Single Player indented flag lines (e.g., "    Ganon Trials: none")
                if (line.StartsWith("  ") && currentWorld == "")
                {
                    var parts = line.Trim().Split(':', 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var condition = parts[0].Trim();
                        var value = parts[1].Trim();

                        worldFlags.Add(new WorldFlag
                        {
                            World = currentWorld,
                            Condition = condition,
                            Value = value
                        });
                    }
                }
            }

            return worldFlags;
        }
        private async Task<List<Entrance>?> Parse_Entrances()
        {

            if (FileContents == null)
            {
                return null;
            }


            var entrances = new List<Entrance>();

            var (start, end) = await FindCategoryRange("Entrances", FileContents);
            if (start == -1) return entrances;

            string? currentWorld = null;

            for (int i = start; i <= end; i++)
            {
                string line = FileContents[i];

                if (string.IsNullOrWhiteSpace(line)) continue;

                // Detect world header for multiplayer
                if (!line.StartsWith("    ") && line.Trim().StartsWith("World", StringComparison.OrdinalIgnoreCase))
                {
                    currentWorld = line.Trim(); // e.g., "World 1"
                    continue;
                }

                // Detect singleplayer section
                if (line.StartsWith("  ") && !line.StartsWith("    "))
                {
                    currentWorld = ""; // Singleplayer: world is empty
                }

                // Multiplayer or singleplayer line
                if (line.StartsWith("    ") || line.StartsWith("  "))
                {
                    var parts = line.Trim().Split(new[] { "->" }, 2, StringSplitOptions.None);
                    if (parts.Length != 2) continue;

                    // Use regex to extract long/short names
                    var pattern = @"^(.*?)\((.*?)\)\s*->\s*(.*?)\((.*?)\)$";
                    var match = Regex.Match(line.Trim(), pattern);
                    if (!match.Success) continue;

                    var entranceLong = match.Groups[1].Value.Trim();
                    var entranceShortRaw = match.Groups[2].Value.Trim();
                    var destinationLong = match.Groups[3].Value.Trim();
                    var destinationShortRaw = match.Groups[4].Value.Trim();

                    // Game code detection (based on raw short values)
                    var fromGame = entranceShortRaw.StartsWith("MM") ? "MM" : "OOT";
                    var toGame = destinationShortRaw.StartsWith("MM") ? "MM" : "OOT";

                    // Remove Game code from (based on raw values)
                    entranceLong = entranceLong.StartsWith("MM") ? entranceLong.Remove(0, 2).Trim() : entranceLong.Remove(0, 4).Trim();
                    entranceShortRaw = entranceShortRaw.StartsWith("MM") ? entranceShortRaw.Remove(0, 2).Trim() : entranceShortRaw.Remove(0, 4).Trim();
                    destinationLong = destinationLong.StartsWith("MM") ? destinationLong.Remove(0, 2).Trim() : destinationLong.Remove(0, 4).Trim();
                    destinationShortRaw = destinationShortRaw.StartsWith("MM") ? destinationShortRaw.Remove(0, 2).Trim() : destinationShortRaw.Remove(0, 4).Trim();

                    // Remove _ from short
                    var entranceShort = entranceShortRaw.Replace('_', ' ').Trim();
                    var destinationShort = destinationShortRaw.Replace('_', ' ').Trim();

                    // Lower case, then upper case First letter for shorts
                    TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
                    entranceShort = textInfo.ToTitleCase(entranceShort.ToLower());
                    destinationShort = textInfo.ToTitleCase(destinationShort.ToLower());
                    

                    entrances.Add(new Entrance
                    {
                        World = currentWorld,
                        FromGame = fromGame,
                        ToGame = toGame,
                        LongEntrance = entranceLong,
                        LongDestination = destinationLong,
                        ShortEntrance = entranceShort,
                        ShortDestination = destinationShort
                    });
                }
            }

            return entrances; 
        }
        private async Task<List<WayOfTheHeroHint>?> Parse_WayOfTheHeroHints()
        {
            if (FileContents == null)
            {
                return null;
            }

            var blockInfo = await FindBlock_Hint("Way of the Hero", FileContents);

            if (blockInfo == null)
                return null;

            if (blockInfo.MultiWorld)
            {
                var block = new BlockInfo();
                List<BlockInfo> blocks = new List<BlockInfo>();

                // Adds inital block info for World 1
                blocks.Add(blockInfo);

                int worldNum = 1;
                do
                {
                    //Starts on world 2
                    worldNum++;
                    block = await FindBlock_Hint("Way of the Hero", FileContents, worldNum, blockInfo.HeaderPosition);

                    if (block != null)
                    {
                        blocks.Add(block);
                    }

                } while (block != null);


                var completeWayOfTheHeroHints = new List<WayOfTheHeroHint>();
                
                for (int i = 0; i < blocks.Count; i++) 
                {
                    Tuple<int, int> range = new Tuple<int, int>(blocks[i].StartLine, blocks[i].EndLine);
                    var multiWayOfTheHeroHints = await AddValues<WayOfTheHeroHint>(range, FileContents);

                    if (multiWayOfTheHeroHints == null)
                        continue;

                    // Adds world info to each item of collection
                    for (int j = 0; j < multiWayOfTheHeroHints.Count; j++)
                    {
                        multiWayOfTheHeroHints[j].World = blocks[i].World;
                    }

                    foreach (var item in multiWayOfTheHeroHints)
                    {
                        completeWayOfTheHeroHints.Add(item);
                    }
                }

                return completeWayOfTheHeroHints;
            }
            else
            {
                Tuple<int, int> range = new Tuple<int, int>(blockInfo.StartLine, blockInfo.EndLine);

                var wayOfTheHeroHints = await AddValues<WayOfTheHeroHint>(range, FileContents);

                if (wayOfTheHeroHints == null)
                    return null;

                // Adds world info to each item of collection
                for (int i = 0; i < wayOfTheHeroHints.Count; i++)
                {
                    wayOfTheHeroHints[i].World = blockInfo.World;
                }

                return wayOfTheHeroHints;
            }
                
        }
        private async Task<List<FoolishHint>?> Parse_FoolishHints()
        {

            if (FileContents == null)
            {
                return null;
            }

            var blockInfo = await FindBlock_Hint("Foolish", FileContents);

            if (blockInfo == null)
                return null;

            if (blockInfo.MultiWorld)
            {
                var block = new BlockInfo();
                List<BlockInfo> blocks = new List<BlockInfo>();

                // Adds inital block info for World 1
                blocks.Add(blockInfo);

                int worldNum = 1;
                do
                {
                    //Starts on world 2
                    worldNum++;
                    block = await FindBlock_Hint("Foolish", FileContents, worldNum, blockInfo.HeaderPosition);

                    if (block != null)
                    {
                        blocks.Add(block);
                    }

                } while (block != null);


                var completeFoolishHints = new List<FoolishHint>();

                for (int i = 0; i < blocks.Count; i++)
                {
                    Tuple<int, int> range = new Tuple<int, int>(blocks[i].StartLine, blocks[i].EndLine);
                    var multiFoolishHints = await AddValues<FoolishHint>(range, FileContents);

                    if (multiFoolishHints == null)
                        continue;

                    // Adds world info to each item of collection
                    for (int j = 0; j < multiFoolishHints.Count; j++)
                    {
                        multiFoolishHints[j].World = blocks[i].World;
                    }

                    foreach (var item in multiFoolishHints)
                    {
                        completeFoolishHints.Add(item);
                    }
                }

                return completeFoolishHints;
            }
            else
            {
                Tuple<int, int> range = new Tuple<int, int>(blockInfo.StartLine, blockInfo.EndLine);

                var foolishHints = await AddValues<FoolishHint>(range, FileContents);

                if (foolishHints == null)
                    return null;

                // Adds world info to each item of collection
                for (int i = 0; i < foolishHints.Count; i++)
                {
                    foolishHints[i].World = blockInfo.World;
                }

                return foolishHints;
            }

        }
        private async Task<List<SpecificHint>?> Parse_SpecificHints()
        {

            if (FileContents == null)
            {
                return null;
            }

            var blockInfo = await FindBlock_Hint("Specific Hints", FileContents);

            if (blockInfo == null)
                return null;

            if (blockInfo.MultiWorld)
            {
                var block = new BlockInfo();
                List<BlockInfo> blocks = new List<BlockInfo>();

                // Adds inital block info for World 1
                blocks.Add(blockInfo);

                int worldNum = 1;
                do
                {
                    //Starts on world 2
                    worldNum++;
                    block = await FindBlock_Hint("Specific Hints", FileContents, worldNum, blockInfo.HeaderPosition);

                    if (block != null)
                    {
                        blocks.Add(block);
                    }

                } while (block != null);


                var completeSpecificHints = new List<SpecificHint>();

                for (int i = 0; i < blocks.Count; i++)
                {
                    Tuple<int, int> range = new Tuple<int, int>(blocks[i].StartLine, blocks[i].EndLine);
                    var multiSpecificHints = await AddValues<SpecificHint>(range, FileContents);

                    if (multiSpecificHints == null)
                        continue;

                    // Adds world info to each item of collection
                    for (int j = 0; j < multiSpecificHints.Count; j++)
                    {
                        multiSpecificHints[j].World = blocks[i].World;
                    }

                    foreach (var item in multiSpecificHints)
                    {
                        completeSpecificHints.Add(item);
                    }
                }

                return completeSpecificHints;
            }
            else
            {
                Tuple<int, int> range = new Tuple<int, int>(blockInfo.StartLine, blockInfo.EndLine);

                var specificHints = await AddValues<SpecificHint>(range, FileContents);

                if (specificHints == null)
                    return null;

                // Adds world info to each item of collection
                for (int i = 0; i < specificHints.Count; i++)
                {
                    specificHints[i].World = blockInfo.World;
                }

                return specificHints;
            }

        }
        private async Task<List<RegionalHint>?> Parse_RegionalHints()
        {

            if (FileContents == null)
            {
                return null;
            }

            var blockInfo = await FindBlock_Hint("Regional Hints", FileContents);

            if (blockInfo == null)
                return null;

            if (blockInfo.MultiWorld)
            {
                var block = new BlockInfo();
                List<BlockInfo> blocks = new List<BlockInfo>();

                // Adds inital block info for World 1
                blocks.Add(blockInfo);

                int worldNum = 1;
                do
                {
                    //Starts on world 2
                    worldNum++;
                    block = await FindBlock_Hint("Regional Hints", FileContents, worldNum, blockInfo.HeaderPosition);

                    if (block != null)
                    {
                        blocks.Add(block);
                    }

                } while (block != null);


                var completeRegionalHints = new List<RegionalHint>();

                for (int i = 0; i < blocks.Count; i++)
                {
                    Tuple<int, int> range = new Tuple<int, int>(blocks[i].StartLine, blocks[i].EndLine);
                    var multiRegionalHints = await AddValues<RegionalHint>(range, FileContents);

                    if (multiRegionalHints == null)
                        continue;

                    // Adds world info to each item of collection
                    for (int j = 0; j < multiRegionalHints.Count; j++)
                    {
                        multiRegionalHints[j].World = blocks[i].World;
                    }

                    foreach (var item in multiRegionalHints)
                    {
                        completeRegionalHints.Add(item);
                    }
                }

                return completeRegionalHints;
            }
            else
            {
                Tuple<int, int> range = new Tuple<int, int>(blockInfo.StartLine, blockInfo.EndLine);

                var regionalHints = await AddValues<RegionalHint>(range, FileContents);

                if (regionalHints == null)
                    return null;

                // Adds world info to each item of collection
                for (int i = 0; i < regionalHints.Count; i++)
                {
                    regionalHints[i].World = blockInfo.World;
                }

                return regionalHints;
            }

        }
        private async Task<List<FoolishRegion>?> Parse_FoolishRegions()
        {

            if (FileContents == null)
            {
                return null;
            }

            var blockInfo = await FindBlock_Hint("Foolish Regions", FileContents);

            if (blockInfo == null)
                return null;

            if (blockInfo.MultiWorld)
            {
                var block = new BlockInfo();
                List<BlockInfo> blocks = new List<BlockInfo>();

                // Adds inital block info for World 1
                blocks.Add(blockInfo);

                int worldNum = 1;
                do
                {
                    //Starts on world 2
                    worldNum++;
                    block = await FindBlock_Hint("Foolish Regions", FileContents, worldNum, blockInfo.HeaderPosition);

                    if (block != null)
                    {
                        blocks.Add(block);
                    }

                } while (block != null);


                var completeFoolishRegions = new List<FoolishRegion>();

                for (int i = 0; i < blocks.Count; i++)
                {
                    Tuple<int, int> range = new Tuple<int, int>(blocks[i].StartLine, blocks[i].EndLine);
                    var multiFoolishRegions = await AddValues<FoolishRegion>(range, FileContents);

                    if (multiFoolishRegions == null)
                        continue;

                    // Adds world info to each item of collection
                    for (int j = 0; j < multiFoolishRegions.Count; j++)
                    {
                        multiFoolishRegions[j].World = blocks[i].World;
                    }

                    foreach (var item in multiFoolishRegions)
                    {
                        completeFoolishRegions.Add(item);
                    }
                }

                return completeFoolishRegions;
            }
            else
            {
                Tuple<int, int> range = new Tuple<int, int>(blockInfo.StartLine, blockInfo.EndLine);

                var foolishRegions = await AddValues<FoolishRegion>(range, FileContents);

                if (foolishRegions == null)
                    return null;

                // Adds world info to each item of collection
                for (int i = 0; i < foolishRegions.Count; i++)
                {
                    foolishRegions[i].World = blockInfo.World;
                }

                return foolishRegions;
            }

        }
        private async Task<List<WayOfTheHeroPath>?> Parse_WayOfTheHeroPaths() 
        {
            if (FileContents == null) 
            {
                return null;
            }
            var block = await FindBlock_Generic(FileContents, "Paths", "Way Of The Hero");

            if (block != null) 
            {
                Tuple<int, int> range = new Tuple<int, int>(block.StartLine, block.EndLine);
                var wayOfTheHeroPaths = await AddValues<WayOfTheHeroPath>(range, FileContents);
                return wayOfTheHeroPaths;
            }
            else 
            {
                return null;
            }
                
        }
        private async Task<List<Sphere>?> Parse_Spheres()
        {
            if (FileContents == null)
            {
                return null;
            }

            BlockInfo? block = new BlockInfo();
            List<BlockInfo> blocks = new List<BlockInfo>();
            int sphereNumber = 0;
            bool firstBlockFound = false;

            // Obtains all the spheres information
            do
            {
                if (!firstBlockFound) 
                {
                    block = await FindBlock_Generic(FileContents, "Spheres", $"Sphere {sphereNumber}", block.EndLine);
                    firstBlockFound = true;
                }
                else 
                {
                    block = await FindBlock_Generic(FileContents, null, $"Sphere {sphereNumber}", block.EndLine);
                }

                if (block != null)
                {
                    blocks.Add(block);
                }

                sphereNumber++;

            } while (block != null);

            // Adds the spheres values
            List<Sphere> sphereList = new List<Sphere>();

            for (int i = 0; i < blocks.Count; i++)
            {
                Tuple<int, int> range = new Tuple<int, int>(blocks[i].StartLine, blocks[i].EndLine);
                var Sphere = await AddValues<Sphere>(range, FileContents);

                if (Sphere != null)
                {
                    foreach (Sphere item in Sphere) 
                    {
                            item.Number = i;
                            sphereList.Add(item);
                    }
                }
            }
            return sphereList;
        }
        private async Task<List<ItemLocation>?> Parse_LocationsList()
        {
            if (FileContents == null)
            {
                return null;
            }

            BlockInfo? block = new BlockInfo();
            List<BlockInfo> blocks = new List<BlockInfo>();

            block = await FindBlock_LocationsList(FileContents, "Location List", 0, true);
            if (block != null)
            {
                int fileLength = block.FileLength;

                // Multiplayer Log
                if (block.MultiWorld)
                {
                    int num = 1;
                    do
                    {
                        blocks.Add(block);
                        block = await FindBlock_LocationsList(FileContents, null, block.EndLine, true, block.World);

                        num++;

                    } while (block != null && block.StartLine < fileLength - 1);
                }
                // Single Player Log
                else
                {
                    do
                    {
                        blocks.Add(block);
                        block = await FindBlock_LocationsList(FileContents, null, block.EndLine, false, block.World, block.HeaderPosition);

                    } while (block != null && block.StartLine < fileLength - 1);
                }

                // Start adding items
                List<ItemLocation> locationItemsList = new List<ItemLocation>();

                for (int i = 0; i < blocks.Count; i++)
                {
                    Tuple<int, int> range = new Tuple<int, int>(blocks[i].StartLine, blocks[i].EndLine);

                    var region = await AddValues<ItemLocation>(range, FileContents);

                    if (region != null)
                    {
                        int itemCount = 1;
                        foreach (ItemLocation item in region)
                        {
                            if (item.Description != null)
                            {
                                if (!string.IsNullOrEmpty(blocks[i].SubHeader)) 
                                {
                                    string[] parts = Regex.Split(blocks[i].SubHeader!, @"\s*\((\d+)\):?\s*$");
                                    if (parts.Length >= 1)
                                    {
                                        item.Region = parts[0];
                                        item.Count = int.Parse(parts[1]);
                                        item.Number = itemCount++;
                                        item.World = blocks[i].World;
                                        locationItemsList.Add(item);
                                    }
                                }
                               
                            }
                        }
                    }
                }
                return locationItemsList;
            }
            else return null;
        }
    }
        #endregion
    
}
