using Campgrounds.Framework.UI;
using StardewValley;
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
        
        public static void OnRepairCarResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "Yes":
                    Game1.globalFadeToBlack(() =>
                    {
                        Game1.freezeControls = true;
                        DelayedAction.playSoundAfterDelay("crafting", 1000);
                        DelayedAction.playSoundAfterDelay("crafting", 1500);
                        DelayedAction.playSoundAfterDelay("crafting", 2000);
                        DelayedAction.playSoundAfterDelay("crafting", 2500);
                        DelayedAction.playSoundAfterDelay("axchop", 3000);
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
    }
}
