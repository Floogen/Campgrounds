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
                Game1.drawObjectDialogue(Campgrounds.modHelper.Translation.Get("messages.camping.talkToClem"));
            }

            return true;
        }

        public static bool HandleCarRepair(GameLocation location, string[] args, Farmer player, Point point)
        {
            if (player.eventsSeen.Any(e => e.EqualsIgnoreCase("PeacefulEnd.Campgrounds.Events.CampingIntro")) is false)
            {
                Game1.drawObjectDialogue(Campgrounds.modHelper.Translation.Get("messages.camping.talkToClemGeneric"));
                return false;
            }

            if (player.Items.ContainsId("(O)787", 5) && player.Items.ContainsId("(O)335", 10) && player.Items.ContainsId("(O)388", 100))
            {
                location.createQuestionDialogue(Campgrounds.modHelper.Translation.Get("messages.garage.repairCar"), location.createYesNoResponses(), CampingHelper.OnRepairCarResponse);
            }
            else
            {
                Game1.drawObjectDialogue(Campgrounds.modHelper.Translation.Get("messages.garage.repairCarMissingItems"));
            }

            return true;
        }

        public static bool HandleVisitorSiteRepair(GameLocation location, string[] args, Farmer farmer, Point point)
        {
            if (ArgUtility.TryGetInt(args, 1, out int siteId, out string error) is false)
            {
                return false;
            }

            if (farmer.eventsSeen.Any(e => e.EqualsIgnoreCase("PeacefulEnd.Campgrounds.Events.CampingIntro")) is false)
            {
                Game1.drawObjectDialogue(Campgrounds.modHelper.Translation.Get("messages.camping.talkToClemGeneric"));
                return false;
            }

            switch (siteId)
            {
                case 1:
                    if (farmer.Items.ContainsId("(O)335", 2) && farmer.Items.ContainsId("(O)388", 100))
                    {
                        location.createQuestionDialogue(Campgrounds.modHelper.Translation.Get("messages.site1.repair"), location.createYesNoResponses(), (Farmer who, string answer) => CampingHelper.OnRepairVisitorSiteResponse(who, answer, siteId));
                    }
                    else
                    {
                        Game1.drawObjectDialogue(Campgrounds.modHelper.Translation.Get("messages.site1.repairMissingItems"));
                    }
                    break;
                case 2:
                    if (farmer.Items.ContainsId("(O)335", 5) && farmer.Items.ContainsId("(O)709", 25) && farmer.Items.ContainsId("(O)388", 200))
                    {
                        location.createQuestionDialogue(Campgrounds.modHelper.Translation.Get("messages.site2.repair"), location.createYesNoResponses(), (Farmer who, string answer) => CampingHelper.OnRepairVisitorSiteResponse(who, answer, siteId));
                    }
                    else
                    {
                        Game1.drawObjectDialogue(Campgrounds.modHelper.Translation.Get("messages.site2.repairMissingItems"));
                    }
                    break;
                case 3:
                    if (farmer.Items.ContainsId("(O)726", 5) && farmer.Items.ContainsId("(O)335", 15) && farmer.Items.ContainsId("(O)709", 50) && farmer.Items.ContainsId("(O)390", 200))
                    {
                        location.createQuestionDialogue(Campgrounds.modHelper.Translation.Get("messages.site3.repair"), location.createYesNoResponses(), (Farmer who, string answer) => CampingHelper.OnRepairVisitorSiteResponse(who, answer, siteId));
                    }
                    else
                    {
                        Game1.drawObjectDialogue(Campgrounds.modHelper.Translation.Get("messages.site3.repairMissingItems"));
                    }
                    break;
            }
            return true;
        }

        public static bool HandleCampShopCounter(GameLocation location, string[] arg2, Farmer farmer, Point point)
        {
            var responses = new Response[]
            {
                new Response("Shop", Campgrounds.modHelper.Translation.Get("dialogues.shop.browseSupplies")),
                new Response("Tents", Campgrounds.modHelper.Translation.Get("dialogues.shop.tentCatalogue")),
                new Response("Leave", Campgrounds.modHelper.Translation.Get("dialogues.shop.leave")),
            };

            location.createQuestionDialogue(Campgrounds.modHelper.Translation.Get("dialogues.shop.greeting"), responses, CampingHelper.OnCampShopCounterResponse);

            return true;
        }

        // Touch actions
        public static void HandleParkClosed(GameLocation location, string[] args, Farmer player, Vector2 tile)
        {
            Game1.drawObjectDialogue(Campgrounds.modHelper.Translation.Get("messages.parkPathwayClosed"));
        }

        public static void HandleCampingExit(GameLocation location, string[] args, Farmer player, Vector2 tile, bool skipLeaveEarlyCheck = false)
        {
            var campsite = Campgrounds.campManager.GetActiveCampsiteFromLocation(location);
            if (skipLeaveEarlyCheck is false && campsite != null && campsite.CookingSpot.HasCookedToday)
            {
                Game1.player.currentLocation.createQuestionDialogue(Campgrounds.modHelper.Translation.Get("messages.camping.leaveWithoutCamping"), location.createYesNoResponses(), CampingHelper.OnLeaveEarlyResponse, null);
                return;
            }

            Response[] answers =
            [
                new Response("HeadToPark", Campgrounds.modHelper.Translation.Get("messages.camping.headBackToPark")),
                new Response("HeadToFarm", Campgrounds.modHelper.Translation.Get("messages.camping.returnHome")),
                new Response("CancelCampingExit", Campgrounds.modHelper.Translation.Get("messages.camping.cancel")),
            ];

            Game1.player.currentLocation.createQuestionDialogue(Campgrounds.modHelper.Translation.Get("messages.camping.leave"), answers, CampingHelper.OnPlayerResponse, null);
        }
    }
}
