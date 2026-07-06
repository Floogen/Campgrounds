using Campgrounds.Framework.UI;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Campgrounds.Framework.Utilities
{
    public static class MapActionHelper
    {
        // Tile actions
        public static bool HandleCampingSiteList(GameLocation location, string[] args, Farmer player, Point point)
        {
            Game1.activeClickableMenu = new CampListMenu();

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
