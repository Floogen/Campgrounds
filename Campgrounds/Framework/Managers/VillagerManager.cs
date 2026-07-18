using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Managers
{
    public class VillagerManager : BaseManager
    {
        public const string GENERIC_VILLAGER_ID = "PeacefulEnd.Campgrounds.Villagers.Generic";
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

        public VillagerData GetGenericData()
        {
            return VillagerData.FirstOrDefault(v => v.Id.EqualsIgnoreCase(GENERIC_VILLAGER_ID));
        }

        public VillagerData GetVillagerData(NPC npc)
        {
            if (npc is null)
            {
                return null;
            }

            return VillagerData.FirstOrDefault(v => v.Id.EqualsIgnoreCase(npc.Name));
        }

        public List<string> GetGenericDialogue(CampDialogue campDialogue)
        {
            var genericVillagerData = GetGenericData();
            switch (campDialogue)
            {
                case CampDialogue.InviteAccepted:
                    return genericVillagerData.InviteDialogueAccepted;
                case CampDialogue.InviteRejected:
                    return genericVillagerData.InviteDialogueRejected;
                case CampDialogue.LikedDayOf:
                    return genericVillagerData.LikedDialogueDayOf;
                case CampDialogue.NeutralDayOf:
                    return genericVillagerData.NeutralDialogueDayOf;
                case CampDialogue.DislikedDayOf:
                    return genericVillagerData.DislikedDialogueDayOf;
                case CampDialogue.LikedDayAfter:
                    return genericVillagerData.LikedDialogueDayAfter;
                case CampDialogue.NeutralDayAfter:
                    return genericVillagerData.NeutralDialogueDayAfter;
                case CampDialogue.DislikedDayAfter:
                    return genericVillagerData.DislikedDialogueDayAfter;
            }

            return new List<string>();
        }

        public List<string> GetDialogue(CampDialogue campDialogue, NPC npc)
        {
            var dialogue = new List<string>();

            var villagerData = GetVillagerData(npc);
            switch (campDialogue)
            {
                case CampDialogue.InviteAccepted:
                    return villagerData.InviteDialogueAccepted;
                case CampDialogue.InviteRejected:
                    return villagerData.InviteDialogueRejected;
                case CampDialogue.LikedDayOf:
                    return villagerData.LikedDialogueDayOf;
                case CampDialogue.NeutralDayOf:
                    return villagerData.NeutralDialogueDayOf;
                case CampDialogue.DislikedDayOf:
                    return villagerData.DislikedDialogueDayOf;
                case CampDialogue.LikedDayAfter:
                    return villagerData.LikedDialogueDayAfter;
                case CampDialogue.NeutralDayAfter:
                    return villagerData.NeutralDialogueDayAfter;
                case CampDialogue.DislikedDayAfter:
                    return villagerData.DislikedDialogueDayAfter;
            }

            // Supplement dialogue with generic values if given one is empty
            if (dialogue.Count == 0)
            {
                dialogue = GetGenericDialogue(campDialogue);
            }

            return dialogue;
        }

        public List<string> GetCampsiteDialogue(CampgroundData campgroundData, NPC npc, bool isDayAfter)
        {
            // Check for campsite specific overrides
            if (isDayAfter is false && campgroundData.DialogueOverrides.Any(o => o.HasOverride(CampDialogue.NeutralDayOf, npc.Name)))
            {
                return campgroundData.DialogueOverrides.First(o => o.Id.EqualsIgnoreCase(npc.Name)).DialogueDayOfOverride;
            }
            if (isDayAfter is true && campgroundData.DialogueOverrides.Any(o => o.HasOverride(CampDialogue.NeutralDayAfter, npc.Name)))
            {
                return campgroundData.DialogueOverrides.First(o => o.Id.EqualsIgnoreCase(npc.Name)).DialogueDayAfterOverride;
            }

            // Get general dialogue
            var villagerData = GetVillagerData(npc);
            if (villagerData.LikedCampgrounds.Any(c => c.EqualsIgnoreCase(campgroundData.Id)))
            {
                return isDayAfter is true ? GetDialogue(CampDialogue.LikedDayAfter, npc) : GetDialogue(CampDialogue.LikedDayOf, npc);
            }
            else if (villagerData.DislikedCampgrounds.Any(c => c.EqualsIgnoreCase(campgroundData.Id)))
            {
                return isDayAfter is true ? GetDialogue(CampDialogue.DislikedDayAfter, npc) : GetDialogue(CampDialogue.DislikedDayOf, npc);
            }

            return isDayAfter is true ? GetDialogue(CampDialogue.NeutralDayAfter, npc) : GetDialogue(CampDialogue.NeutralDayOf, npc);
        }

        public string GetGameReadyDialogue(List<string> dialogue)
        {
            return string.Join("#$b#", dialogue);
        }

        public Character GetInvitedCharacter(Farmer who)
        {
            if (who.modData.ContainsKey(INVITED_CAMPSITE_INVITE_MOD_DATA_ID))
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
