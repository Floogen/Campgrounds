using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.Objects;
using Campgrounds.Framework.UI.Messages;
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
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Campgrounds.Framework.Managers
{
    public class CampingManager : BaseManager
    {
        public const string CACHED_BUFF_IDS_MOD_DATA_ID = "Campgrounds.Buffs.Cache.Id";
        public const string LAST_CAMPSITE_SLEPT_MOD_DATA_ID = "Campgrounds.Campsite.LastCampsite.Slept.Id";

        public List<CampgroundData> CampgroundData { get { return _campgroundData; } set { FilterCampgroundData(value); } }
        private List<CampgroundData> _campgroundData = new List<CampgroundData>();

        public List<CampingTentData> CampingTentData { get { return _campingTentData; } set { FilterCampingTentsData(value); } }
        private List<CampingTentData> _campingTentData = new List<CampingTentData>();        

        public List<CampfireFoodData> CampfireFoodData { get { return _campfireFoodData; } set { FilterCampfireFoodsData(value); } }
        private List<CampfireFoodData> _campfireFoodData = new List<CampfireFoodData>();

        public List<Campsite> ActiveCampsites { get; private set; } = new List<Campsite>();

        public CampingManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            CampgroundData = helper.GameContent.Load<List<CampgroundData>>(Campgrounds.CAMPGROUND_DATA_PATH);
            CampingTentData = helper.GameContent.Load<List<CampingTentData>>(Campgrounds.CAMPING_TENTS_DATA_PATH);
            CampfireFoodData = helper.GameContent.Load<List<CampfireFoodData>>(Campgrounds.CAMPFIRE_FOODS_DATA_PATH);
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

                Farmer guestFarmer = null;
                if (location.farmers.Count > 1)
                {
                    guestFarmer = location.farmers.Skip(1).First();
                }

                ActiveCampsites.Add(new Campsite(location.farmers.First(), campground, guestFarmer));
            }
        }

        public void StartTraveling(Farmer who, CampgroundData campgroundData, Character guest = null)
        {
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

            Campgrounds.messageManager.Messages.Add(new TravelMessage(campgroundData));

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
