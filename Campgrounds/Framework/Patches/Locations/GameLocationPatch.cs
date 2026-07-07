using Campgrounds.Framework.Objects;
using Campgrounds.Framework.Patches;
using Campgrounds.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using System;
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
            harmony.Patch(AccessTools.Method(_object, nameof(GameLocation.draw), new[] { typeof(SpriteBatch) }), postfix: new HarmonyMethod(GetType(), nameof(DrawPostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(GameLocation.isActionableTile), new[] { typeof(int), typeof(int), typeof(Farmer) }), postfix: new HarmonyMethod(GetType(), nameof(IsActionableTilePostfix)));
        }

        private static void DrawPostfix(GameLocation __instance, SpriteBatch b)
        {
            if (__instance.Name == "PeacefulEnd.Campgrounds.ContentPatcher_CindersapParkGarage" && CampingHelper.IsCarRepaired() is false)
            {
                float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3f * 64f, 2f * 64f + yOffset)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.095401f);
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3.65f * 64f, 2.6f * 64f + yOffset)), new Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.09541f);
            }
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