using Campgrounds.Framework.UI;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static void HandleCampingExit(GameLocation location, string[] args, Farmer player, Vector2 tile)
        {
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
