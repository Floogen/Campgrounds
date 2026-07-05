using Campgrounds.Framework.Objects;
using Campgrounds.Framework.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace Campgrounds.Framework.Patches.Locations
{
    internal class GameLocationPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(GameLocation);

        public GameLocationPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(GameLocation.isActionableTile), new[] { typeof(int), typeof(int), typeof(Farmer) }), postfix: new HarmonyMethod(GetType(), nameof(IsActionableTilePostfix)));
        }

        private static void IsActionableTilePostfix(GameLocation __instance, ref bool __result, int xTile, int yTile, Farmer who)
        {
            if (Campgrounds.campManager.CampgroundData.Any(c => c.Id == __instance.Name))
            {
                foreach (CampingTent campingTent in __instance.largeTerrainFeatures.Where(t => t is CampingTent))
                {
                    if (campingTent.GetEntranceTile() == new Vector2(xTile, yTile))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
    }
}