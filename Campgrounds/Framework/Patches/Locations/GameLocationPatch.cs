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
                DrawQuestionMarkBubble(b, new Vector2(2.9f, 2f), 0.095401f);
            }
            else if (__instance.Name == "PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark")
            {
                // Draw campfires
                foreach (var campfireTile in Campgrounds.visitorManager.CampfireTiles)
                {
                    var x = campfireTile.X;
                    var y = campfireTile.Y;

                    float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 16 - 4, y * 64 - 8)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 0.0008f);
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 - 12, y * 64)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2047) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 0.0009f);
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 - 20, y * 64 + 12)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2077) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 0.001f);
                }

                // Draw sign "?" text
                if (NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(1)) is false)
                {
                    DrawQuestionMarkBubble(b, new Vector2(18.4f, 33.4f), 2f);
                }
                if (NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(2)) is false)
                {
                    DrawQuestionMarkBubble(b, new Vector2(21.4f, 19.4f), 2f);
                }
                if (NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(3)) is false)
                {
                    DrawQuestionMarkBubble(b, new Vector2(45.4f, 36.4f), 2f);
                }
            }
        }

        private static void DrawQuestionMarkBubble(SpriteBatch b, Vector2 tile, float layer)
        {
            float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tile.X * 64f, tile.Y * 64f + yOffset)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, layer);
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((tile.X + 0.65f) * 64f, (tile.Y + 0.6f) * 64f + yOffset)), new Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, layer + 0.00001f);
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