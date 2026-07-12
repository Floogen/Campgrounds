using Campgrounds.Framework.UI;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using System;
using System.Linq;
using xTile.Tiles;

namespace Campgrounds.Framework.Utilities
{
    public static class MapActionHelper
    {        
        public static bool HandleCampingSiteList(GameLocation location, string[] args, Farmer player, Point point)
        {
            if (player.eventsSeen.Any(e => e.EqualsIgnoreCase("PeacefulEnd.Campgrounds.Events.CampingIntro")))
            {
                Game1.activeClickableMenu = new CampListMenu();
            }
            else
            {
                Game1.drawObjectDialogue("You should talk to Clementine first before using the camping board.");
            }

            return true;
        }

        public static bool HandleCarRepair(GameLocation location, string[] args, Farmer player, Point point)
        {
            if (player.Items.ContainsId("(O)787", 5) && player.Items.ContainsId("(O)335", 10) && player.Items.ContainsId("(O)388", 100))
            {
                location.createQuestionDialogue("Repair the car?", location.createYesNoResponses(), CampingHelper.OnRepairCarResponse);
            }
            else
            {
                Game1.drawObjectDialogue("The car is in bad shape. You could get it working again with 5 battery packs, 10 iron bars and 100 wood.");
            }

            return true;
        }

        public static bool HandleVisitorSiteRepair(GameLocation location, string[] args, Farmer farmer, Point point)
        {
            if (ArgUtility.TryGetInt(args, 1, out int siteId, out string error) is false)
            {
                return false;
            }

            switch (siteId)
            {
                case 1:
                    if (farmer.Items.ContainsId("(O)335", 2) && farmer.Items.ContainsId("(O)388", 100))
                    {
                        location.createQuestionDialogue("Clean up the campsite?", location.createYesNoResponses(), (Farmer who, string answer) => CampingHelper.OnRepairVisitorSiteResponse(who, answer, siteId));
                    }
                    else
                    {
                        Game1.drawObjectDialogue("This campsite is in disrepair. You can get it cleaned up with 2 iron bars and 100 wood.");
                    }
                    break;
                case 2:
                    if (farmer.Items.ContainsId("(O)335", 5) && farmer.Items.ContainsId("(O)709", 25) && farmer.Items.ContainsId("(O)388", 200))
                    {
                        location.createQuestionDialogue("Fix up the campsite?", location.createYesNoResponses(), (Farmer who, string answer) => CampingHelper.OnRepairVisitorSiteResponse(who, answer, siteId));
                    }
                    else
                    {
                        Game1.drawObjectDialogue("This campsite needs a lot of work. You can restore it with 5 iron bars, 25 hardwood and 200 wood.");
                    }
                    break;
                case 3:
                    if (farmer.Items.ContainsId("(O)726", 5) && farmer.Items.ContainsId("(O)335", 15) && farmer.Items.ContainsId("(O)709", 50) && farmer.Items.ContainsId("(O)390", 200))
                    {
                        location.createQuestionDialogue("Rebuild the campsite?", location.createYesNoResponses(), (Farmer who, string answer) => CampingHelper.OnRepairVisitorSiteResponse(who, answer, siteId));
                    }
                    else
                    {
                        Game1.drawObjectDialogue("This campsite has been completely reclaimed by nature. You can rebuild it with 5 pine tar, 15 iron bars, 50 hardwood and 200 stone.");
                    }
                    break;
            }
            return true;
        }

        // Touch actions
        public static void HandleCampingExit(GameLocation location, string[] args, Farmer player, Vector2 tile, bool skipLeaveEarlyCheck = false)
        {
            var campsite = Campgrounds.campManager.GetActiveCampsiteFromLocation(location);
            if (skipLeaveEarlyCheck is false && campsite != null && campsite.CookingSpot.HasCookedToday)
            {
                Game1.player.currentLocation.createQuestionDialogue("Leave without camping? You will not receive any buffs.", location.createYesNoResponses(), CampingHelper.OnLeaveEarlyResponse, null);
                return;
            }

            Response[] answers =
            [
                new Response("HeadToPark", "Head back to the park"),
                new Response("HeadToFarm", "Return home"),
                new Response("CancelCampingExit", "Cancel")
            ];

            Game1.player.currentLocation.createQuestionDialogue("Leave the campground?", answers, CampingHelper.OnPlayerResponse, null);
        }
    }
}
