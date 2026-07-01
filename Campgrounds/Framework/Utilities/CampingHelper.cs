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
        public static void OnPlayerResponse(Farmer who, string answer)
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
