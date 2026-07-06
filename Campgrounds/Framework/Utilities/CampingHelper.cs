using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
