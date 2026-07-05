using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.Objects;
using Campgrounds.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Campgrounds.Framework.Managers
{
    public class CampingManager : BaseManager
    {
        public List<CampgroundData> CampgroundData { get { return _campgroundData; } set { FilterCampgroundData(value); } }
        private List<CampgroundData> _campgroundData = new List<CampgroundData>();

        public List<CampingTentData> CampingTentData { get { return _campingTentData; } set { FilterCampingTentsData(value); } }
        private List<CampingTentData> _campingTentData = new List<CampingTentData>();

        public bool IsTraveling { get; private set; }
        private TravelMessage _travelMessage;

        public CampingManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            CampgroundData = helper.GameContent.Load<List<CampgroundData>>(Campgrounds.CAMPGROUND_DATA_PATH);
            CampingTentData = helper.GameContent.Load<List<CampingTentData>>(Campgrounds.CAMPING_TENTS_DATA_PATH);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (_travelMessage is not null)
            {
                _travelMessage.Update();
            }
        }

        private void FilterCampgroundData(List<CampgroundData> campgroundData)
        {
            foreach (var campground in campgroundData)
            {
                var isValidData = campground.IsValid();
                if (isValidData.Result is false)
                {
                    monitor.LogOnce($"Skipping invalid CampgroundData with name \"{campground.Id}\": {isValidData.Error}", LogLevel.Warn);
                }
            }

            _campgroundData = campgroundData.Where(c => c.IsValid().Result is true).ToList();
        }

        private void FilterCampingTentsData(List<CampingTentData> campingTentData)
        {
            foreach (var campingTent in campingTentData)
            {
                var isValidData = campingTent.IsValid();
                if (isValidData.Result is false)
                {
                    monitor.LogOnce($"Skipping invalid CampingTentData with name \"{campingTent.Id}\": {isValidData.Error}", LogLevel.Warn);
                }
            }

            _campingTentData = campingTentData.Where(c => c.IsValid().Result is true).ToList();
        }

        public void StartTraveling(CampgroundData campgroundData)
        {
            if (IsTraveling is true)
            {
                return;
            }
            IsTraveling = true;

            // Add tents and other camping equipment
            if (HandleCampsiteSetup(campgroundData) is false)
            {
                IsTraveling = false;
                return;
            }

            _travelMessage = new TravelMessage(campgroundData);

            // Adjust the time by the CampgroundData.TravelTimeInHours
            Game1.timeOfDay += campgroundData.TravelTimeInHours * 100;
        }

        public void StopTraveling()
        {
            IsTraveling = false;
            _travelMessage = null;
        }

        public bool HandleCampsiteSetup(CampgroundData campgroundData)
        {
            var location = Game1.getLocationFromName(campgroundData.Id);
            if (location is null)
            {
                monitor.LogOnce($"The campgrounds map with name {campgroundData.Id} does not exist!", LogLevel.Warn);
                return false;
            }

            // Get tent tiles
            var layer = location.Map.GetLayer("Back");

            Vector2? playerTentTile = null;
            Vector2? guestTentTile = null;

            Direction playerTentDirection = Direction.South;
            Direction guestTentDirection = Direction.South;

            Vector2? cookingSpotTile = null;

            for (int x = 0; x < layer.LayerWidth; x++)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    if (location.doesTileHaveProperty(x, y, "IsCampingSpot", "Back") != null)
                    {
                        if (location.doesTileHaveProperty(x, y, "IsForGuest", "Back") == "T")
                        {
                            if (Enum.TryParse<Direction>(location.doesTileHaveProperty(x, y, "CampingDirection", "Back"), out var direction))
                            {
                                guestTentDirection = direction;
                            }

                            guestTentTile = new Vector2(x, y);
                        }
                        else
                        {
                            if (Enum.TryParse<Direction>(location.doesTileHaveProperty(x, y, "CampingDirection", "Back"), out var direction))
                            {
                                playerTentDirection = direction;
                            }

                            playerTentTile = new Vector2(x, y);
                        }
                    }

                    if (location.doesTileHaveProperty(x, y, "IsCookingSpot", "Back") != null)
                    {
                        cookingSpotTile = new Vector2(x, y);
                    }
                }
            }

            if (playerTentTile is null)
            {
                monitor.LogOnce($"The campgrounds map with name {campgroundData.Id} is missing the player's tent spot (IsCampingSpot tile property on Back layer)", LogLevel.Warn);
                return false;
            }
            if (guestTentTile is null)
            {
                monitor.LogOnce($"The campgrounds map with name {campgroundData.Id} is missing the guest's tent spot (IsCampingSpot and IsForGuest tile property on Back layer)", LogLevel.Warn);
                return false;
            }
            if (cookingSpotTile is null)
            {
                monitor.LogOnce($"The campgrounds map with name {campgroundData.Id} is missing a cooking spot (IsCookingSpot tile property on Back layer)", LogLevel.Warn);
                return false;
            }

            // Place the tents
            if (!location.isTerrainFeatureAt((int)playerTentTile.Value.X, (int)playerTentTile.Value.Y))
            {
                location.largeTerrainFeatures.Add(new CampingTent(playerTentTile.Value, playerTentDirection, CampingTentData.First()));
            }
            if (!location.isTerrainFeatureAt((int)guestTentTile.Value.X, (int)guestTentTile.Value.Y))
            {
                location.largeTerrainFeatures.Add(new CampingTent(guestTentTile.Value, guestTentDirection, CampingTentData.First()));
            }

            // Place the cooking spot
            if (!location.objects.ContainsKey(cookingSpotTile.Value))
            {
                var cookingSpotObject = new Torch("278", bigCraftable: true)
                {
                    IsOn = true,
                    Fragility = 2
                };
                location.objects.Add(cookingSpotTile.Value, cookingSpotObject);
                cookingSpotObject.initializeLightSource(cookingSpotTile.Value);
            }

            return true;
        }

        public void HandleForageSpawning()
        {
            foreach (var campground in CampgroundData.Where(c => c.ForceForageRefreshOnVisit))
            {
                var location = Game1.getLocationFromName(campground.Id);
                if (location is null)
                {
                    continue;
                }

                // Remove any previous forage items
                foreach (var tile in location.objects.Keys)
                {
                    if (location.objects[tile] is not null && location.objects[tile].isForage())
                    {
                        location.objects.Remove(tile);
                    }
                }
                location.numberOfSpawnedObjectsOnMap = 0;

                // Spawn in new forage items
                location.spawnObjects();
            }
        }
    }
}
