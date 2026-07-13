using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Pets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Managers
{
    public class TentManager : BaseManager
    {
        public const string CURRENT_TENT_MOD_DATA_ID = "Campgrounds.Tents.Active.Id";
        public const string UNLOCKED_TENT_MOD_DATA_ID = "Campgrounds.Tents.Unlocked.Id";
        public const string STARTER_TENT_ID = "PeacefulEnd.Campgrounds.Tents.StarterTent";

        public List<CampingTentData> CampingTentData { get { return _campingTentData; } set { FilterCampingTentsData(value); } }
        private List<CampingTentData> _campingTentData = new List<CampingTentData>();

        public TentManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            CampingTentData = helper.GameContent.Load<List<CampingTentData>>(Campgrounds.CAMPING_TENTS_DATA_PATH);
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

        public CampingTentData GetTentDataById(string tentDataId)
        {
            return CampingTentData.FirstOrDefault(t => t.Id.EqualsIgnoreCase(tentDataId));
        }

        public CampingTentData GetStarterTent()
        {
            return _campingTentData.FirstOrDefault(t => t.Id.EqualsIgnoreCase(STARTER_TENT_ID));
        }

        public void SetCurrentTent(Farmer who, CampingTentData campingTentData)
        {
            who.modData[CURRENT_TENT_MOD_DATA_ID] = campingTentData.Id;
        }

        public CampingTentData GetCurrentTent(Farmer who)
        {
            CampingTentData tentData = null;
            if (who.modData.ContainsKey(CURRENT_TENT_MOD_DATA_ID))
            {
                tentData = GetTentDataById(who.modData[CURRENT_TENT_MOD_DATA_ID]);
            }

            if (tentData is null)
            {
                tentData = GetStarterTent();
                SetCurrentTent(who, tentData);
            }

            return tentData;
        }

        public void AddUnlockedTent(Farmer who, CampingTentData campingTentData)
        {
            var unlockedCampingTents = GetUnlockedTents(who);
            unlockedCampingTents.Add(campingTentData);

            who.modData[UNLOCKED_TENT_MOD_DATA_ID] = JsonSerializer.Serialize(unlockedCampingTents);
        }

        public List<CampingTentData> GetUnlockedTents(Farmer who)
        {
            var unlockedCampingTents = new List<CampingTentData>();
            if (who.modData.ContainsKey(UNLOCKED_TENT_MOD_DATA_ID))
            {
                unlockedCampingTents = JsonSerializer.Deserialize<List<CampingTentData>>(who.modData[UNLOCKED_TENT_MOD_DATA_ID]);
            }

            // Ensure Starter Tent is added to the list
            if (unlockedCampingTents.Count == 0)
            {
                unlockedCampingTents.Add(GetStarterTent());
            }

            return unlockedCampingTents;
        }
    }
}
