using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Data.Visitors;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using xTile;
using xTile.Layers;

namespace Campgrounds.Framework.Managers
{
    public class VisitorManager : BaseManager
    {
        public const string NEXT_VISIT_COOLDOWN_MOD_DATA_ID = "Campgrounds.NextVisit.Cooldown.Id";
        public const int REQUIRED_DAYS_BETWEEN_VISIT = 7;

        // The order in which the park's visitor spots claim a visitor. Earlier spots pick first.
        public static readonly VisitorSpots[] VISITOR_SPOT_FILL_ORDER = new VisitorSpots[]
        {
            VisitorSpots.SW,
            VisitorSpots.NW,
            VisitorSpots.SE
        };

        public List<VisitorData> VisitorData { get { return _visitorData; } set { FilterVisitorData(value); } }
        private List<VisitorData> _visitorData = new List<VisitorData>();

        public Dictionary<VisitorSpots, VisitorData> ActiveVisitorSpots { get; set; } = new Dictionary<VisitorSpots, VisitorData>();

        public List<Vector2> CampfireTiles = new List<Vector2>();
        public List<Vector2> SmokeTiles = new List<Vector2>();

        private double _smokeTimer = 0f;

        private Dictionary<string, int> _visitorIdToNextVisitCooldown = new Dictionary<string, int>();

        public VisitorManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.Saving += OnSaving; ;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            VisitorData = helper.GameContent.Load<List<VisitorData>>(Campgrounds.PARK_VISITORS_DATA_PATH);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Clear current active visitors
            ActiveVisitorSpots.Clear();

            // Get the latest cache for recent visitors
            if (Game1.player.modData.ContainsKey(NEXT_VISIT_COOLDOWN_MOD_DATA_ID))
            {
                _visitorIdToNextVisitCooldown = JsonSerializer.Deserialize<Dictionary<string, int>>(Game1.player.modData[NEXT_VISIT_COOLDOWN_MOD_DATA_ID]);
            }
            else
            {
                // No cached cooldowns for this save, so clear any carried over from a previously loaded one
                _visitorIdToNextVisitCooldown.Clear();
            }

            // Get today's game date
            SDate today = SDate.Now();

            // Determine visitors for the three available spots in the park
            var filteredVisitorData = new List<VisitorData>();
            foreach (var visitorData in VisitorData)
            {
                // Skip if VisitorData has no preferred date today and is still within the visit cooldown
                if (visitorData.HasPreferredDate(today) is false && _visitorIdToNextVisitCooldown.TryGetValue(visitorData.Id, out int nextVisitCooldownDay) && nextVisitCooldownDay - today.DaysSinceStart >= 0)
                {
                    continue;
                }

                filteredVisitorData.Add(visitorData);
            }

            // Determine the visitor for each repaired spot, in fill order.
            // Unrepaired spots are skipped entirely so they don't consume a visitor or burn its cooldown.
            foreach (var visitorSpot in VISITOR_SPOT_FILL_ORDER)
            {
                if (IsVisitorSpotRepaired(visitorSpot) is false)
                {
                    continue;
                }

                var parkSpotVisitorData = GetVisitorsForToday(filteredVisitorData, today, visitorSpot);
                if (parkSpotVisitorData != null)
                {
                    SetActiveVisitor(visitorSpot, today, filteredVisitorData, parkSpotVisitorData);
                }
            }

            // Invalidate the park's map to force the patches to apply
            helper.GameContent.InvalidateCache(Campgrounds.CINDERSAP_PARK_MAP_PATH);

            // Update any lighting / fires due to visitorTile property changes
            UpdateVisitorSpotTileProperties();

            // Handle adding / removing visitors
            HandleVisitors();
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            Game1.player.modData[NEXT_VISIT_COOLDOWN_MOD_DATA_ID] = JsonSerializer.Serialize(_visitorIdToNextVisitCooldown);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.game1.IsActive is false)
            {
                return;
            }

            if (_smokeTimer <= 0f)
            {
                _smokeTimer = 1000f;

                var location = Game1.getLocationFromName("PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark");
                if (location is null)
                {
                    return;
                }

                foreach (var smokeTile in SmokeTiles)
                {
                    Utility.addSmokePuff(location, smokeTile * 64f + new Vector2(24f, -16f));
                }
            }
            else
            {
                _smokeTimer -= Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            // Update lighting
            UpdateCampfireLighting();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(Campgrounds.CINDERSAP_PARK_MAP_PATH))
            {
                e.Edit(asset =>
                {
                    // Check which visitor campsite(s) have been unlocked
                    var editor = asset.AsMap();

                    // Load the active version of the park map (for StandardVisitorSettings usage)
                    Map activeParkMap = helper.GameContent.Load<Map>(Campgrounds.CINDERSAP_PARK_ACTIVE_MAP_PATH);

                    foreach (var visitorSpot in VISITOR_SPOT_FILL_ORDER)
                    {
                        if (IsVisitorSpotRepaired(visitorSpot) is false || ActiveVisitorSpots.ContainsKey(visitorSpot) is false)
                        {
                            continue;
                        }

                        var visitorData = ActiveVisitorSpots[visitorSpot];
                        if (visitorData.AdvancedVisitorSettings != null)
                        {
                            foreach (var mapPatch in visitorData.AdvancedVisitorSettings.MapPatches)
                            {
                                editor.PatchMap(
                                    source: helper.GameContent.Load<Map>(mapPatch.MapPath),
                                    sourceArea: mapPatch.FromArea,
                                    targetArea: mapPatch.ToArea,
                                    patchMode: mapPatch.PatchMode
                                );
                            }
                        }
                        else if (visitorData.StandardVisitorSettings != null)
                        {
                            var visitorSpotArea = GetVisitorSpotArea(visitorSpot);
                            editor.PatchMap(
                                source: activeParkMap,
                                sourceArea: visitorSpotArea,
                                targetArea: visitorSpotArea,
                                patchMode: PatchMapMode.Replace
                            );
                        }
                    }
                });
            }
        }

        /// <summary>Gets the park's site number for the given visitor spot, as used by the repair tile action and world state keys.</summary>
        public static int GetVisitorSpotSiteId(VisitorSpots visitorSpot)
        {
            switch (visitorSpot)
            {
                case VisitorSpots.SW:
                    return 1;
                case VisitorSpots.NW:
                    return 2;
                case VisitorSpots.SE:
                    return 3;
            }

            return 0;
        }

        /// <summary>Gets the park map area covered by the given visitor spot.</summary>
        public static Rectangle GetVisitorSpotArea(VisitorSpots visitorSpot)
        {
            switch (visitorSpot)
            {
                case VisitorSpots.SW:
                    return new Rectangle(0, 25, 22, 26);
                case VisitorSpots.NW:
                    return new Rectangle(0, 0, 35, 25);
                case VisitorSpots.SE:
                    return new Rectangle(40, 0, 29, 51);
            }

            return Rectangle.Empty;
        }

        /// <summary>Gets whether the player has repaired the given visitor spot's site.</summary>
        public static bool IsVisitorSpotRepaired(VisitorSpots visitorSpot)
        {
            return NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(GetVisitorSpotSiteId(visitorSpot)));
        }

        private void SetActiveVisitor(VisitorSpots visitorSpot, SDate today, List<VisitorData> visitorDataCache, VisitorData visitorData)
        {
            int daysRequiredBetweenRepeatVisit = REQUIRED_DAYS_BETWEEN_VISIT;
            if (visitorData.DaysRequiredBetweenVisits is not null)
            {
                daysRequiredBetweenRepeatVisit = visitorData.DaysRequiredBetweenVisits.Value;
            }

            _visitorIdToNextVisitCooldown[visitorData.Id] = today.DaysSinceStart + daysRequiredBetweenRepeatVisit;

            visitorDataCache.Remove(visitorData);
            ActiveVisitorSpots[visitorSpot] = visitorData;
        }

        private void UpdateVisitorSpotTileProperties()
        {
            var location = Game1.getLocationFromName("PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark");
            if (location is null)
            {
                return;
            }

            // Clear any campfire lighting
            foreach (var lightSource in Game1.currentLightSources.Where(l => l.Value.Id.ContainsIgnoreCase("campfireParkLightSource_")).ToList())
            {
                Game1.currentLightSources.Remove(lightSource.Key);
            }

            // Clear current tracked fire tiles
            CampfireTiles.Clear();
            SmokeTiles.Clear();

            // Update the park tiles
            var layer = location.Map.GetLayer("Buildings");
            for (int x = 0; x < layer.LayerWidth; x++)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    if (location.doesTileHaveProperty(x, y, "IsCampfire", "Buildings") != null)
                    {
                        CampfireTiles.Add(new Vector2(x, y));
                        if (location.doesTileHaveProperty(x, y, "HasSmoke", "Buildings") != null)
                        {
                            SmokeTiles.Add(new Vector2(x, y));
                        }
                    }
                }
            }

            // Update lighting
            UpdateCampfireLighting();
        }

        private void HandleVisitors()
        {
            var location = Game1.getLocationFromName("PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark");
            if (location is null)
            {
                return;
            }

            // Clear any existing visitors from the map
            foreach (var npc in location.characters.ToList())
            {
                // Exclude the park's character
                if (npc.Name.EqualsIgnoreCase("PeacefulEnd.Campgrounds.Characters.Caretaker"))
                {
                    continue;
                }

                NPCHelper.ReturnNPCToSchedule(npc);
            }

            // Add required visitors to map
            var manualVisitorTiles = new List<VisitorTile>();
            var visitorSpotToSpawnTiles = new Dictionary<VisitorSpots, List<VisitorTile>>()
            {
                { VisitorSpots.SW, new List<VisitorTile>() },
                { VisitorSpots.NW, new List<VisitorTile>() },
                { VisitorSpots.SE, new List<VisitorTile>() }
            };

            var layer = location.Map.GetLayer("Back");
            for (int x = 0; x < layer.LayerWidth; x++)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    if (location.doesTileHaveProperty(x, y, "IsVisitorSpawn", "Back") != null && Enum.TryParse<VisitorSpots>(location.doesTileHaveProperty(x, y, "VisitorSpot", "Back"), true, out var visitorSpot))
                    {
                        Direction direction;
                        if (Enum.TryParse<Direction>(location.doesTileHaveProperty(x, y, "VisitorFacingDirection", "Back"), true, out direction) is false)
                        {
                            direction = Direction.North;
                        }

                        if (ActiveVisitorSpots.ContainsKey(visitorSpot))
                        {
                            visitorSpotToSpawnTiles[visitorSpot].Add(new VisitorTile() { Tile = new Vector2(x, y), Direction = direction });
                        }
                    }
                    else if (string.IsNullOrEmpty(location.doesTileHaveProperty(x, y, "SpawnVisitorName", "Back")) is false)
                    {
                        Direction direction;
                        if (Enum.TryParse<Direction>(location.doesTileHaveProperty(x, y, "VisitorFacingDirection", "Back"), true, out direction) is false)
                        {
                            direction = Direction.North;
                        }

                        manualVisitorTiles.Add(new VisitorTile() { Tile = new Vector2(x, y), Direction = direction, SpawnVisitorName = location.doesTileHaveProperty(x, y, "SpawnVisitorName", "Back") });
                    }
                }
            }

            // Spawn in visitors added via VisitorData.StandardVisitorSettings
            foreach (var visitorSpot in visitorSpotToSpawnTiles)
            {
                if (IsVisitorSpotRepaired(visitorSpot.Key) && ActiveVisitorSpots.ContainsKey(visitorSpot.Key))
                {
                    AddVisitors(visitorSpot.Key, visitorSpot.Value, location);
                }
            }

            // Spawn in visitors added in via map tile property "SpawnVisitorName"
            foreach (var visitorTile in manualVisitorTiles)
            {
                var visitor = Game1.getCharacterFromName(visitorTile.SpawnVisitorName);
                if (visitor != null)
                {
                    NPCHelper.WarpAndSetDialogue(visitor, location, visitorTile.Tile, faceDirection: visitorTile.Direction, freeze: true);
                }
            }
        }

        private void AddVisitors(VisitorSpots visitorSpot, List<VisitorTile> visitorTiles, GameLocation location)
        {
            if (ActiveVisitorSpots[visitorSpot].StandardVisitorSettings is null)
            {
                return;
            }

            int visitorIndex = 0;
            var visitorNames = ActiveVisitorSpots[visitorSpot].StandardVisitorSettings.Visitors;
            foreach (var visitorTile in visitorTiles.OrderBy(x => Game1.random.Next()))
            {
                // Skip past any names that don't resolve, so an unknown NPC doesn't consume a spawn tile
                NPC visitor = null;
                while (visitor is null && visitorIndex < visitorNames.Count)
                {
                    visitor = Game1.getCharacterFromName(visitorNames[visitorIndex]);
                    if (visitor is null)
                    {
                        monitor.LogOnce($"Unable to find the NPC \"{visitorNames[visitorIndex]}\", given by the VisitorData \"{ActiveVisitorSpots[visitorSpot].Id}\".", LogLevel.Warn);
                    }

                    visitorIndex++;
                }

                if (visitor is null)
                {
                    return;
                }

                NPCHelper.WarpAndSetDialogue(visitor, location, visitorTile.Tile, faceDirection: visitorTile.Direction, freeze: true);
            }
        }

        private void UpdateCampfireLighting()
        {
            foreach (var campfireTile in CampfireTiles)
            {
                float yOffset = 32f;

                var lightSource = new LightSource($"campfireParkLightSource_{Guid.NewGuid()}", LightSource.sconceLight, new Vector2(campfireTile.X * 64f + 32f, campfireTile.Y * 64f + yOffset), 2.5f, new Color(0, 80, 160));
                Game1.currentLightSources.Add(lightSource);
            }
        }

        private void FilterVisitorData(List<VisitorData> visitorData)
        {
            foreach (var visitors in visitorData)
            {
                var isValidData = visitors.IsValid();
                if (isValidData.Result is false)
                {
                    monitor.LogOnce($"Skipping invalid VisitorData with name \"{visitors.Id}\": {isValidData.Error}", LogLevel.Warn);
                }
            }

            _visitorData = visitorData.Where(d => d.IsValid().Result is true).ToList();
        }

        public IEnumerable<VisitorData> GetPreferredDateMapPatchOnlyVisitorData(IEnumerable<VisitorData> visitors, SDate date, VisitorSpots visitorSpot)
        {
            var allPreferredDateVisitors = visitors
                .Where(d => d.HasPreferredDate(date))
                .Where(d => d.AdvancedVisitorSettings is not null && d.AdvancedVisitorSettings.RequiredSpot == visitorSpot);

            return allPreferredDateVisitors;
        }

        public IEnumerable<VisitorData> GetPreferredDateVisitorData(IEnumerable<VisitorData> visitors, SDate date, VisitorSpots visitorSpot)
        {
            var allPreferredDateVisitors = visitors
                .Where(d => d.HasPreferredDate(date))
                .Where(d => d.StandardVisitorSettings is not null || (d.AdvancedVisitorSettings is not null && d.AdvancedVisitorSettings.RequiredSpot == visitorSpot));

            return allPreferredDateVisitors;
        }

        public IEnumerable<VisitorData> GetPreferredDayVisitorData(IEnumerable<VisitorData> visitors, SDate date, VisitorSpots visitorSpot)
        {
            var allPreferredDateVisitors = visitors
                .Where(d => d.HasPreferredDay(date))
                .Where(d => d.StandardVisitorSettings is not null || (d.AdvancedVisitorSettings is not null && d.AdvancedVisitorSettings.RequiredSpot == visitorSpot));

            return allPreferredDateVisitors;
        }

        public IEnumerable<VisitorData> GetFlexibleVisitorData(IEnumerable<VisitorData> visitors, VisitorSpots visitorSpot)
        {
            var allFlexibleVisitors = visitors
                .Where(d => d.StandardVisitorSettings is not null || (d.AdvancedVisitorSettings is not null && d.AdvancedVisitorSettings.RequiredSpot == visitorSpot));

            return allFlexibleVisitors;
        }

        /// <summary>
        /// VisitorData is selected based on following order of priority:<br/>
        /// - Randomly select AdvancedVisitorSettings with PreferredDates that match the game's date for the specific spot.<br/>
        /// - Randomly select any with PreferredDates that match the game's date.<br/>
        /// - Randomly select any with PreferredDays that match the game's day.<br/>
        /// - Randomly select any remaining eligible data.<br/>
        /// </summary>
        /// <param name="visitors"></param>
        /// <param name="today"></param>
        /// <param name="visitorSpot"></param>
        /// <returns></returns>
        public VisitorData GetVisitorsForToday(IEnumerable<VisitorData> visitors, SDate today, VisitorSpots visitorSpot)
        {
            var parkSpotVisitorData = GetRandomVisitor(GetPreferredDateMapPatchOnlyVisitorData(visitors, today, visitorSpot));
            if (parkSpotVisitorData is null)
            {
                parkSpotVisitorData = GetRandomVisitor(GetPreferredDateVisitorData(visitors, today, visitorSpot));
                if (parkSpotVisitorData is null)
                {
                    parkSpotVisitorData = GetRandomVisitor(GetPreferredDayVisitorData(visitors, today, visitorSpot));

                    if (parkSpotVisitorData is null)
                    {
                        parkSpotVisitorData = GetRandomVisitor(GetFlexibleVisitorData(visitors, visitorSpot));
                    }
                }
            }

            return parkSpotVisitorData;
        }

        private VisitorData GetRandomVisitor(IEnumerable<VisitorData> visitors)
        {
            SDate today = SDate.Now();
            VisitorData visitorData = null;

            // Check those with PreferredDates first
            visitorData = visitors.Where(d => d.PreferredDates is not null).OrderBy(x => Game1.random.Next()).FirstOrDefault(d => d.CanVisitToday(today));
            if (visitorData is not null)
            {
                return visitorData;
            }

            // Check those with PreferredDays next
            visitorData = visitors.Where(d => d.PreferredDays is not null).OrderBy(x => Game1.random.Next()).FirstOrDefault(d => d.CanVisitToday(today));
            if (visitorData is not null)
            {
                return visitorData;
            }

            // Lastly check for any that CanVisitToday
            visitorData = visitors.OrderBy(x => Game1.random.Next()).FirstOrDefault(d => d.CanVisitToday(today));
            if (visitorData is not null)
            {
                return visitorData;
            }

            return visitorData;
        }
    }
}
