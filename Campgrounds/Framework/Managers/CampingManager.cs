using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.Objects;
using Campgrounds.Framework.UI.Messages;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Campgrounds.Framework.Managers
{
    public class CampingManager : BaseManager
    {
        public static string TOTAL_NIGHTS_GONE_CAMPING_STAT_ID = "PeacefulEnd.Campgrounds_TotalNightsGoneCamping";

        public const string CACHED_BUFF_IDS_MOD_DATA_ID = "Campgrounds.Buffs.Cache.Id";
        public const string LAST_CAMPSITE_SLEPT_MOD_DATA_ID = "Campgrounds.Campsite.LastCampsite.Slept.Id";

        public List<CampgroundData> CampgroundData { get { return _campgroundData; } set { FilterCampgroundData(value); } }
        private List<CampgroundData> _campgroundData = new List<CampgroundData>();

        public List<CampfireFoodData> CampfireFoodData { get { return _campfireFoodData; } set { FilterCampfireFoodsData(value); } }
        private List<CampfireFoodData> _campfireFoodData = new List<CampfireFoodData>();

        public List<Campsite> ActiveCampsites { get; private set; } = new List<Campsite>();

        public CampingManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            CampgroundData = helper.GameContent.Load<List<CampgroundData>>(Campgrounds.CAMPGROUND_DATA_PATH);
            CampfireFoodData = helper.GameContent.Load<List<CampfireFoodData>>(Campgrounds.CAMPFIRE_FOODS_DATA_PATH);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            FindActiveCampsites();
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            // Sanitize any campsites before saving to prevent serialization issues
            foreach (var campsite in ActiveCampsites)
            {
                campsite.Sanitize();
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            HandleActiveCampsites();
            HandleForageSpawning();
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            var campsite = GetActiveCampsiteFromLocation(e.OldLocation);
            if (campsite is not null)
            {
                EndCampingTrip(e.OldLocation);
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

        private void FilterCampfireFoodsData(List<CampfireFoodData> campfireFoodData)
        {
            foreach (var campfireFood in campfireFoodData)
            {
                var isValidData = campfireFood.IsValid();
                if (isValidData.Result is false)
                {
                    monitor.LogOnce($"Skipping invalid CampfireFoodData with name \"{campfireFood.Id}\": {isValidData.Error}", LogLevel.Warn);
                }
            }

            _campfireFoodData = campfireFoodData.Where(c => c.IsValid().Result is true).ToList();
        }

        public void FindActiveCampsites()
        {
            foreach (var campground in CampgroundData)
            {
                var location = Game1.getLocationFromName(campground.Id);
                if (location is null || location.farmers.Count == 0)
                {
                    continue;
                }

                Character guest = null;
                if (location.farmers.Count > 1)
                {
                    guest = location.farmers.Where(f => f != Game1.player).First();
                }
                else
                {
                    guest = Campgrounds.villagerManager.GetInvitedCharacter(Game1.player);
                }

                ActiveCampsites.Add(new Campsite(Game1.player, campground, guest));
            }
        }

        public CampfireFoodData GetCampfireFoodDataById(string id)
        {
            return CampfireFoodData.FirstOrDefault(c => c.Id.EqualsIgnoreCase(id));
        }

        public string GetLocationNameFromDataId(string campgroundDataId)
        {
            string locationName = string.Empty;
            if (Game1.locationData.ContainsKey(campgroundDataId))
            {
                locationName = Game1.locationData[campgroundDataId].DisplayName;
            }

            return locationName;
        }

        public void StartTraveling(Farmer who, CampgroundData campgroundData)
        {
            var guest = Campgrounds.villagerManager.GetInvitedCharacter(who);
            var campsite = new Campsite(who, campgroundData, guest);
            if (ActiveCampsites.Any(c => c.Data == campgroundData) is false)
            {
                // Add tents and other camping equipment
                if (campsite.HandleCampsiteSetup() is false)
                {
                    return;
                }
                ActiveCampsites.Add(campsite);

                who.modDataForSerialization[CampingManager.LAST_CAMPSITE_SLEPT_MOD_DATA_ID] = string.Empty;
            }

            // Put the invited villager on cooldown now that the trip is happening
            if (guest is NPC)
            {
                Campgrounds.villagerManager.RecordInvite(who, guest);
            }

            Campgrounds.messageManager.Messages.Add(new TravelMessage(campgroundData));
            if (guest is Farmer guestFarmer && guestFarmer is not null)
            {
                // TODO: Send net message so other farmer can trigger their own TravelMessage
            }

            // Adjust the time by the CampgroundData.TravelTimeInHours
            Game1.timeOfDay += campgroundData.TravelTimeInHours * 100;
        }

        public void StartSleep(GameLocation location)
        {
            var campsite = ActiveCampsites.FirstOrDefault(c => c.GetLocation() == location);
            if (campsite is null)
            {
                return;
            }
            campsite.Sleep();
        }

        public void EndCampingTrip(GameLocation location)
        {
            var campsite = ActiveCampsites.FirstOrDefault(c => c.GetLocation() == location);
            if (campsite is null)
            {
                return;
            }
            campsite.HandleExit();

            ActiveCampsites.Remove(campsite);
        }

        public Campsite GetActiveCampsiteFromLocation(GameLocation location)
        {
            return ActiveCampsites.FirstOrDefault(c => c.GetLocation() == location);
        }

        public string GetLastCampsiteSleptIn(Farmer who)
        {
            if (who.modDataForSerialization.ContainsKey(LAST_CAMPSITE_SLEPT_MOD_DATA_ID))
            {
                return who.modDataForSerialization[LAST_CAMPSITE_SLEPT_MOD_DATA_ID];
            }

            return string.Empty;
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

        public void HandleActiveCampsites()
        {
            foreach (var campsite in ActiveCampsites)
            {
                campsite.HandleNewDay();
            }
        }
    }
}
