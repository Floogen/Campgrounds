using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Managers
{
    public class VillagerManager : BaseManager
    {
        public const string INVITED_CAMPSITE_INVITE_MOD_DATA_ID = "Campgrounds.Campsite.Invite.Id";

        public List<VillagerData> VillagerData { get { return _villagerData; } set { FilterVillageData(value); } }
        private List<VillagerData> _villagerData = new List<VillagerData>();

        public VillagerManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            VillagerData = helper.GameContent.Load<List<VillagerData>>(Campgrounds.VILLAGER_DATA_PATH);
        }

        private void FilterVillageData(List<VillagerData> villagerData)
        {
            foreach (var villager in villagerData)
            {
                var isValidData = villager.IsValid();
                if (isValidData.Result is false)
                {
                    monitor.LogOnce($"Skipping invalid VillagerData with name \"{villager.Id}\": {isValidData.Error}", LogLevel.Warn);
                }
            }

            _villagerData = villagerData.Where(c => c.IsValid().Result is true).ToList();
        }

        public Character GetInvitedCharacter()
        {
            if (Game1.player.modData.ContainsKey(INVITED_CAMPSITE_INVITE_MOD_DATA_ID))
            {
                return Game1.getCharacterFromName(who.modData[INVITED_CAMPSITE_INVITE_MOD_DATA_ID]);
            }

            return null;
        }

        public void SetInvitedCharacter(Farmer who, Character character = null)
        {
            who.modData[INVITED_CAMPSITE_INVITE_MOD_DATA_ID] = string.Empty;
            if (character != null)
            {
                who.modData[INVITED_CAMPSITE_INVITE_MOD_DATA_ID] = character.Name;
            }
        }
    }
}
