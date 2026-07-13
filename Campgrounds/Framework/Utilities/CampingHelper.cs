using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.UI;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace Campgrounds.Framework.Utilities
{
    public static class CampingHelper
    {
        public static void OnTentSleepResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "Yes":
                    Campgrounds.campManager.StartSleep(who.currentLocation);
                    break;
                case "No":
                    break;
            }
        }

        public static void OnRepairVisitorSiteResponse(Farmer who, string answer, int siteId)
        {
            if  (answer != "Yes")
            {
                return;
            }

            switch (siteId)
            {
                case 1:
                    NetWorldState.addWorldStateIDEverywhere(GetCindersapParkVisitorParkKey(siteId));

                    Game1.player.Items.ReduceId("(O)335", 2);
                    Game1.player.Items.ReduceId("(O)388", 100);
                    break;
                case 2:
                    NetWorldState.addWorldStateIDEverywhere(GetCindersapParkVisitorParkKey(siteId));

                    Game1.player.Items.ReduceId("(O)335", 5);
                    Game1.player.Items.ReduceId("(O)709", 25);
                    Game1.player.Items.ReduceId("(O)388", 200);
                    break;
                case 3:
                    NetWorldState.addWorldStateIDEverywhere(GetCindersapParkVisitorParkKey(siteId));

                    Game1.player.Items.ReduceId("(O)726", 5);
                    Game1.player.Items.ReduceId("(O)335", 15);
                    Game1.player.Items.ReduceId("(O)709", 50);
                    Game1.player.Items.ReduceId("(O)390", 200);
                    break;
            }

            Game1.globalFadeToBlack(() =>
            {
                Game1.freezeControls = true;
                DelayedAction.playSoundAfterDelay("woodWhack", 1000, pitch: 100);
                DelayedAction.playSoundAfterDelay("crafting", 1500);
                DelayedAction.playSoundAfterDelay("crafting", 2500, pitch: 50);
                DelayedAction.playSoundAfterDelay("woodWhack", 3000);
                DelayedAction.playSoundAfterDelay("crafting", 3500, pitch: 100);
                DelayedAction.playSoundAfterDelay("achievement", 4800);

                Game1.viewportFreeze = true;
                Game1.viewport.X = -10000;
                Game1.pauseThenDoFunction(5000, () =>
                {
                    Game1.globalFadeToClear();
                    Game1.viewportFreeze = false;
                    Game1.freezeControls = false;
                });

                Campgrounds.modHelper.GameContent.InvalidateCache(Campgrounds.CINDERSAP_PARK_MAP_PATH);
            });
        }

        public static string GetCindersapParkVisitorParkKey(int siteId)
        {
            return $"CINDERSAP_PARK_VISITOR_SITE_{siteId}";
        }

        public static void OnRepairCarResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "Yes":
                    Game1.globalFadeToBlack(() =>
                    {
                        Game1.freezeControls = true;
                        DelayedAction.playSoundAfterDelay("hammer", 1000);
                        DelayedAction.playSoundAfterDelay("clank", 1500);
                        DelayedAction.playSoundAfterDelay("hammer", 2000);
                        DelayedAction.playSoundAfterDelay("hammer", 2500);
                        DelayedAction.playSoundAfterDelay("clank", 3000);
                        DelayedAction.playSoundAfterDelay("Ship", 3200);

                        Game1.viewportFreeze = true;
                        Game1.viewport.X = -10000;
                        Game1.pauseThenDoFunction(4000, () =>
                        {
                            Game1.globalFadeToClear();
                            Game1.viewportFreeze = false;
                            Game1.freezeControls = false;
                        });

                        AttemptRepairCarMapTiles(who.currentLocation, forceRepair: true);
                    });
                    Game1.player.Items.ReduceId("(O)787", 5);
                    Game1.player.Items.ReduceId("(O)335", 10);
                    Game1.player.Items.ReduceId("(O)388", 100);
                    break;
                case "No":
                    break;
            }
        }

        public static void OnLeaveWithoutRationsResponse(Farmer who, string answer, CampListMenu campListMenu)
        {
            switch (answer)
            {
                case "Yes":
                    campListMenu.StartTravelingToCampsite(skipRationCheck: true);
                    break;
                case "No":
                    Game1.activeClickableMenu = campListMenu;
                    break;
            }
        }        
        
        public static void OnOverrideCampingInviteResponse(Farmer who, string answer, NPC npc)
        {
            switch (answer)
            {
                case "Yes":
                    Campgrounds.villagerManager.SetInvitedCharacter(who, null);
                    npc.tryToReceiveActiveObject(who, probe: false);
                    break;
                case "No":
                    break;
            }
        }

        public static void OnWalkieTalkieCheckParkResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "Yes":
                    var cindersapPark = Game1.getLocationFromName("PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark");
                    if (cindersapPark is null)
                    {
                        return;
                    }

                    // Close the dialogue menu before the event
                    Game1.activeClickableMenu = null;

                    // Get all current NPCs in the park and create actors of them
                    var actorsPlace = new List<string>();
                    foreach (NPC npc in cindersapPark.characters)
                    {
                        var tile = npc.TilePoint;
                        actorsPlace.Add($"{npc.Name} {tile.X} {tile.Y} {npc.FacingDirection}");
                    }

                    // Create the warp for actors (in case event is started on Farm)
                    var actorsWarp = new List<string>();
                    foreach (NPC npc in cindersapPark.characters)
                    {
                        var tile = npc.TilePoint;
                        actorsWarp.Add($"warp {npc.Name} {tile.X} {tile.Y} true/facedirection {npc.Name} {npc.FacingDirection} true");
                    }
                    var actorsWarpParsed = "";
                    if (actorsWarp.Count > 0)
                    {
                        actorsWarpParsed = $"/{string.Join("/", actorsWarp)}";
                    }

                    string script = $"none/-1000 -1000/farmer -100 -100 2 {string.Join(" ", actorsPlace)}/skippable/viewport -1000 -1000/changeLocation PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark"
                        + $"{actorsWarpParsed}"
                        + "/message \"Let's see who is here...\""
                        + "/viewport 13 41 true/pause 1500/message \"Site 1\""
                        + "/viewport 11 10 true/pause 1500/message \"Site 2\""
                        + "/viewport 58 37 true/pause 1500/message \"Site 3\""
                        + "/end";

                    FadeScreenHelper.StartFadeIn(afterFadeInAction: () =>
                    {
                        // Cache and adjust settings that need to be restored after event
                        Vector2 originalPosition = Game1.player.Position;
                        var zoomLevel = Game1.options.desiredBaseZoomLevel;
                        Game1.options.desiredBaseZoomLevel = 0.75f;

                        var walkieTalkieEvent = new Event(script);
                        walkieTalkieEvent.onEventFinished += () => RestoreAfterWalkieTalkieEvent(originalPosition, zoomLevel);
                        Game1.currentLocation.startEvent(walkieTalkieEvent);

                        DelayedAction.functionAfterDelay(() =>
                        {
                            FadeScreenHelper.ImmediatelyStopFade();
                        }, 100);

                    });
                    break;
                case "No":
                    break;
            }
        }

        private static void RestoreAfterWalkieTalkieEvent(Vector2 originalPosition, float zoomLevel)
        {
            // Restore original location after a slight delay (100 ticks) due to odd behavior with Event in FarmHouse
            // Patching after the event logic might be a more reliable or custom event key?
            DelayedAction.functionAfterDelay(() =>
            {
                Game1.player.Position = originalPosition;
            }, 100);

            Game1.options.desiredBaseZoomLevel = zoomLevel;
        }

        public static bool IsCarRepaired()
        {
            return NetWorldState.checkAnywhereForWorldStateID("PeacefulEnd.Campgrounds.CarRepaired");
        }

        public static void AttemptRepairCarMapTiles(GameLocation location, bool forceRepair = false)
        {
            if (forceRepair is true)
            {
                NetWorldState.addWorldStateIDEverywhere("PeacefulEnd.Campgrounds.CarRepaired");
            }
            else if (IsCarRepaired() is false)
            {
                return;
            }

            location.updateMap();
            location.removeTileProperty(3, 4, "Buildings", "Action");

            int baseOffset = 4;
            for (int i = 0; i < 4; i++)
            {
                var offset = baseOffset - i;
                location.setMapTile(4 + i, 3, 61 - offset, "Buildings", "Cindersap Interior");
                location.setMapTile(4 + i, 4, 84 - offset, "Buildings", "Cindersap Interior");
                location.setMapTile(4 + i, 5, 107 - offset, "Buildings", "Cindersap Interior");
                location.setMapTile(4 + i, 6, 130 - offset, "Buildings", "Cindersap Interior");
                location.setMapTile(4 + i, 7, 153 - offset, "Buildings", "Cindersap Interior");
                location.setMapTile(4 + i, 8, 176 - offset, "Buildings", "Cindersap Interior");
                location.setMapTile(4 + i, 9, 199 - offset, "Buildings", "Cindersap Interior");
            }
        }

        public static void OnLeaveEarlyResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "Yes":
                    var campsite = Campgrounds.campManager.GetActiveCampsiteFromLocation(who.currentLocation);
                    if (campsite is not null)
                    {
                        campsite.ClearBuffs();
                    }

                    MapActionHelper.HandleCampingExit(who.currentLocation, new string[0], who, who.Tile, skipLeaveEarlyCheck: true);
                    break;
                case "No":
                    break;
            }
        }

        public static void OnPlayerResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "HeadToPark":
                    Campgrounds.campManager.EndCampingTrip(who.currentLocation);

                    Game1.warpFarmer("PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark", 36, 30, 2);
                    break;
                case "HeadToFarm":
                    Campgrounds.campManager.EndCampingTrip(who.currentLocation);

                    var farmerHome = Utility.getHomeOfFarmer(who);
                    if (farmerHome is not null)
                    {
                        // Warp to front door of home
                        var position = farmerHome.getFrontDoorSpot();
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

        public static void OnCampShopCounterResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "Shop":
                    Utility.TryOpenShopMenu("PeacefulEnd.Campgrounds.Shops.CampShop", "PeacefulEnd.Campgrounds.Characters.Caretaker");
                    break;
                case "Tents":
                    Game1.activeClickableMenu = new TentListMenu();
                    break;
            }
        }
    }
}
