using Campgrounds.Framework.Objects;
using Campgrounds.Framework.Patches;
using Campgrounds.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

namespace Campgrounds.Framework.Patches.Characters
{
    internal class NPCPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(NPC);

        public NPCPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(NPC.tryToReceiveActiveObject), new[] { typeof(Farmer), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(TryToReceiveActiveObjectPrefix)));
        }

        [HarmonyPriority(Priority.VeryHigh)]
        private static bool TryToReceiveActiveObjectPrefix(NPC __instance, ref bool __result, Farmer who, bool probe = false)
        {
            if (probe is true || who.ActiveObject is null || !who.ActiveObject.QualifiedItemId.EqualsIgnoreCase("(O)PeacefulEnd.Campgrounds.Items.VisitorPass"))
            {
                return true;
            }

            // Check they have an entry in VillagerData or use the VillagerManager.GENERIC_VILLAGER_ID default (only if they are marriage candidates)
            var villageData = Campgrounds.villagerManager.GetVillagerData(__instance);
            if (villageData is null && __instance.datable.Value is true)
            {
                villageData = Campgrounds.villagerManager.GetGenericData();
            }

            // Check if anyone else has been invited with Camping Pass
            var currentInvitedCharacter = Campgrounds.villagerManager.GetInvitedCharacter(who);
            if (currentInvitedCharacter is not null)
            {
                if (currentInvitedCharacter == __instance)
                {
                    Game1.drawObjectDialogue(_helper.Translation.Get("dialogues.general.repeatInvited", new
                    {
                        npcName = __instance.displayName
                    }));
                }
                else if (Campgrounds.villagerManager.CanBeInvited(who, __instance) is false)
                {
                    var cooldownDialogue = new List<string>() { _helper.Translation.Get("dialogues.general.defaultInviteOnCooldown") };
                    if (villageData is not null && villageData.InviteDialogueCooldown.Count > 0)
                    {
                        cooldownDialogue = villageData.InviteDialogueCooldown;
                    }
                    Game1.DrawDialogue(new Dialogue(__instance, null, Campgrounds.villagerManager.GetGameReadyDialogue(cooldownDialogue)));
                }
                else
                {
                    string question = _helper.Translation.Get("dialogues.general.alreadyInvitedButInviteOther", new
                    {
                        currentNpcName = currentInvitedCharacter.displayName,
                        newNpcName = __instance.displayName
                    });
                    who.currentLocation.createQuestionDialogue(question, who.currentLocation.createYesNoResponses(), (Farmer who, string answer) => CampingHelper.OnOverrideCampingInviteResponse(who, answer, __instance));
                }

                __result = false;
                return false;
            }
            else
            {
                if (villageData is not null && villageData.HasRequirements() is true)
                {
                    if (Campgrounds.villagerManager.CanBeInvited(who, __instance) is false)
                    {
                        var cooldownDialogue = new List<string>() { _helper.Translation.Get("dialogues.general.defaultInviteOnCooldown") };
                        if (villageData is not null && villageData.InviteDialogueCooldown.Count > 0)
                        {
                            cooldownDialogue = villageData.InviteDialogueCooldown;
                        }
                        Game1.DrawDialogue(new Dialogue(__instance, null, Campgrounds.villagerManager.GetGameReadyDialogue(cooldownDialogue)));

                        __result = false;
                        return false;
                    }

                    Campgrounds.villagerManager.SetInvitedCharacter(who, __instance);
                    Game1.DrawDialogue(new Dialogue(__instance, null, Campgrounds.villagerManager.GetGameReadyDialogue(villageData.InviteDialogueAccepted)));
                }
                else
                {
                    List<string> rejectDialogue = new List<string>() { _helper.Translation.Get("dialogues.general.defaultReject") };
                    if (villageData is not null)
                    {
                        rejectDialogue = villageData.InviteDialogueRejected;
                    }
                    Game1.DrawDialogue(new Dialogue(__instance, null, Campgrounds.villagerManager.GetGameReadyDialogue(rejectDialogue)));

                    __result = false;
                    return false;
                }
            }

            __result = true;
            return false;
        }
    }
}