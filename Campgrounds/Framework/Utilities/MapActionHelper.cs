using Campgrounds.Framework.UI;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using System.Linq;

namespace Campgrounds.Framework.Utilities
{
    public static class MapActionHelper
    {
        // Tile actions
        public static bool HandleCampShop(GameLocation location, string[] args, Farmer player, Point point)
        {
            Utility.TryOpenShopMenu("PeacefulEnd.Campgrounds.Shops.CampShop", null, playOpenSound: true);

            return true;
        }
        
        public static bool HandleCampingSiteList(GameLocation location, string[] args, Farmer player, Point point)
        {
            Game1.activeClickableMenu = new CampListMenu();

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
