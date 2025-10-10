using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpoilerToTrackerConverter.SpoilerLog.Enumerators
{
    public static class SortByExtensions
    {
        public static string CustomToString(this SortBy sortBy)
        {
            return sortBy switch
            {
                SortBy.Default => "Default",
                SortBy.GameSettingsAlphabetic => "A-Z",
                SortBy.GameSettingsReverseAlphabetic => "Z-A",
                SortBy.GameSettingsLogOrder => "Log Order",
                SortBy.EntrancesLong => "Long Game OOT/MM A-Z",
                SortBy.EntrancesShort => "Short Game OOT/MM A-Z",
                SortBy.EntrancesLongAlphabetic => "Long A-Z",
                SortBy.EntrancesLongReverseAlphabetic => "Long Z-A",
                SortBy.EntrancesShortAlphabetic => "Short A-Z",
                SortBy.EntrancesShortReverseAlphabetic => "Short Z-A",
                SortBy.EntrancesLongGame => "Long Game OOT/MM A-Z",
                SortBy.EntrancesShortGame => "Short Game OOT/MM A-Z",
                SortBy.EntrancesLongReverseGame => "Long Game MM/OOT A-Z",
                SortBy.EntrancesShortReverseGame => "Short Game MM/OOT A-Z",
                SortBy.TricksAlphabetic => "A-Z",
                SortBy.TricksReverseAlphabetic => "Z-A",
                SortBy.TricksDifficulty => "Tricks by Difficulty",
                SortBy.TricksLogOrder => "Tricks by Log Order",
                SortBy.StartingItemsName => "Item A-Z",
                SortBy.StartingItemsValue => "Value",
                SortBy.StartingItemsCount => "Count",
                SortBy.GlitchesAlphabetic => "A-Z",
                SortBy.GlitchesReverseAlphabetic => "Z-A",
                SortBy.GlitchesDifficulty => "Glitches by Difficulty",
                SortBy.GlitchesLogOrder => "Glitches by Log Order",
                SortBy.WayOfTheHeroHintsWorld => "Worlds",
                SortBy.WayOfTheHeroHintsItems => "Itempoly A-Z",
                SortBy.WayOfTheHeroHintsLocation => "Location A-Z",
                SortBy.FoolishHintsWorld => "Worlds",
                SortBy.FoolishHintsGossip => "Gossip A-Z",
                SortBy.FoolishHintsLocation => "Location A-Z",
                SortBy.SpecificHintsWorld => "Worlds",
                SortBy.SpecificHintsGossip => "Gossip A-Z",
                SortBy.SpecificHintsLocation => "Location A-Z",
                SortBy.SpecificHintsItem => "Items A-Z",
                SortBy.RegionalHintsWorld => "Worlds",
                SortBy.RegionalHintsGossip => "Gossip A-Z",
                SortBy.RegionalHintsRegion => "Region A-Z",
                SortBy.RegionalHintsItem => "Items A-Z",
                SortBy.FoolishRegionsWorld => "Worlds",
                SortBy.FoolishRegionsRegion => "Region A-Z",
                SortBy.FoolishRegionsCount => "Region A-Z",
                SortBy.WayOfTheHeroPathsLogOrder => "Log Order",
                SortBy.WayOfTheHeroPathsWorld => "Worlds",
                SortBy.WayOfTheHeroPathsDescription => "Description A-Z",
                SortBy.WayOfTheHeroPathsPlayer => "Player A-Z",
                SortBy.WayOfTheHeroPathsItem => "Itempoly A-Z",
                SortBy.SpheresWorld => "Worlds",
                SortBy.SpheresType => "Type A-Z",
                SortBy.SpheresNumber => "Number",
                SortBy.SpheresLocation => "Location",
                SortBy.SpheresItem => "Items",
                SortBy.LocationsListWorld => "World",
                SortBy.LocationsListGame => "Game A-Z",
                SortBy.LocationsListRegion => "Region A-Z",
                SortBy.LocationsListCount => "Count",
                SortBy.LocationsListNumber => "Number",
                SortBy.LocationsListDescription => "Description A-Z",
                SortBy.LocationsListPlayer => "Player",
                SortBy.LocationsListItem => "Itempoly A-Z",

                // Catch-all for any undefined or unexpected enum values
                _ => "Unknown"

            };
        }
    }
}
