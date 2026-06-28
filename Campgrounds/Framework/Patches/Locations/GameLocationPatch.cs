using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using System;

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
            harmony.Patch(AccessTools.Method(_object, nameof(GameLocation.performTouchAction), new[] { typeof(string), typeof(Vector2) }), postfix: new HarmonyMethod(GetType(), nameof(PerformTouchActionPostfix)));
        }

        private static void PerformTouchActionPostfix(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            if (Game1.eventUp || string.IsNullOrEmpty(fullActionString))
            {
                return;
            }

            var actionName = fullActionString.Split(' ')[0];
            if (actionName.Equals("CampingExit", System.StringComparison.OrdinalIgnoreCase))
            {
                if (Game1.player.CurrentItem is null)
                {
                    return;
                }

                Response[] answers =
                [
                    new Response("HeadToPark", "Head back to the park"),
                    new Response("HeadToFarm", "Return home"),
                    new Response("CancelCampingExit", "Cancel")
                ];

                Game1.player.currentLocation.createQuestionDialogue("Leave the campground?", answers, OnPlayerResponse, null);
            }
        }

        private static void OnPlayerResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "HeadToPark":
                    // TODO: End camping logic

                    Game1.warpFarmer("PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark", 36, 30, 2);
                    break;
                case "HeadToFarm":
                    // TODO: End camping logic

                    var farmerHome = Utility.getHomeOfFarmer(who);
                    if (farmerHome is not null)
                    {
                        // Warp to front door of home
                        Point position = farmerHome.getFrontDoorSpot();
                        Game1.warpFarmer("Farm", position.X, position.Y, 2);
                    }
                    else
                    {
                        Game1.warpHome();
                    }
                    break;
                case "CancelCampingExit":
                    return;
            }
        }
    }
}