using Campgrounds.Framework.Objects;
using Campgrounds.Framework.Patches;
using Campgrounds.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
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
            if (probe is true || who.ActiveObject is null || !who.ActiveObject.QualifiedItemId.EqualsIgnoreCase("(O)PeacefulEnd.Campgrounds.Items.CampingPass"))
            {
                return true;
            }

            // Check if anyone else has been invited with Camping Pass
            var currentInvitedCharacter = Campgrounds.villagerManager.GetInvitedCharacter();
            if (currentInvitedCharacter is not null)
            {
                if (currentInvitedCharacter == __instance)
                {
                    Game1.drawObjectDialogue($"You already invited {__instance.displayName} to go camping.");
                }
                else
                {
                    who.currentLocation.createQuestionDialogue($"You already invited {currentInvitedCharacter.displayName} to go camping. Invite {__instance.displayName} instead?", who.currentLocation.createYesNoResponses(), (Farmer who, string answer) => CampingHelper.OnOverrideCampingInviteResponse(who, answer, __instance));
                }

                __result = false;
                return false;
            }
            else
            {
                Campgrounds.villagerManager.SetInvitedCharacter(__instance);
                Game1.DrawDialogue(new Dialogue(__instance, null, "Thank you for the invite! It will be fun to go camping."));
            }

            __result = true;
            return false;
        }
    }
}