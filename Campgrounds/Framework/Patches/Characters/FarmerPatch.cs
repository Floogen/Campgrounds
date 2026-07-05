using Campgrounds.Framework.Objects;
using Campgrounds.Framework.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace Campgrounds.Framework.Patches.Characters
{
    internal class FarmerPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(Farmer);

        private static bool _isApplyingBonusExperience = false;

        public FarmerPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Farmer.gainExperience), new[] { typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(GainExperiencePostfix)));
        }

        private static void GainExperiencePostfix(Farmer __instance, int which, int howMuch)
        {
            if (_isApplyingBonusExperience is false && HasXPBoostBuff(__instance))
            {
                float multiplier = 0;
                if (__instance.hasBuff("PeacefulEnd.Campgrounds.Buffs.LowXPBuff"))
                {
                    multiplier = 0.25f;
                }
                else if (__instance.hasBuff("PeacefulEnd.Campgrounds.Buffs.MedXPBuff"))
                {
                    multiplier = 0.5f;
                }
                else if (__instance.hasBuff("PeacefulEnd.Campgrounds.Buffs.HighXPBuff"))
                {
                    multiplier = 0.75f;
                }
                else if (__instance.hasBuff("PeacefulEnd.Campgrounds.Buffs.MaxXPBuff"))
                {
                    multiplier = 1f;
                }

                _isApplyingBonusExperience = true;
                __instance.gainExperience(which, (int)(howMuch * multiplier));
                _isApplyingBonusExperience = false;
            }
        }

        private static bool HasXPBoostBuff(Farmer who)
        {
            if (who.hasBuff("PeacefulEnd.Campgrounds.Buffs.LowXPBuff") || who.hasBuff("PeacefulEnd.Campgrounds.Buffs.MedXPBuff") || who.hasBuff("PeacefulEnd.Campgrounds.Buffs.HighXPBuff") || who.hasBuff("PeacefulEnd.Campgrounds.Buffs.MaxXPBuff"))
            {
                return true;
            }

            return false;
        }
    }
}