using Campgrounds.Framework.Managers;
using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.Objects;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Network;
using System.Linq;

namespace Campgrounds.Framework.Utilities
{
    public static class EventHelper
    {
        public static void GiveRationsCommand(Event @event, string[] args, EventContext context)
        {
            if (!ArgUtility.TryGetInt(args, 1, out int amount, out string error))
            {
                context.LogErrorAndSkip(error);
                return;
            }

            // Add rations
            Campgrounds.currencyManager.ChangeCurrencyBalance(Currency.CampRations, amount);

            // Tell the event to advance to the next command
            @event.CurrentCommand++;
        }

        public static void GiveCampsiteMapCommand(Event @event, string[] args, EventContext context)
        {
            if (!ArgUtility.TryGet(args, 1, out string campgroundDataId, out string error))
            {
                context.LogErrorAndSkip(error);
                return;
            }

            var campgroundData = Campgrounds.campManager.CampgroundData.FirstOrDefault(c => c.Id.EqualsIgnoreCase(campgroundDataId));
            if (campgroundData is not null)
            {
                // Give map
                Campgrounds.itemManager.UnlockSpecialAndHoldAboveHead(Campgrounds.itemManager.GetCampsiteMapUnlockKey(campgroundDataId), ItemManager.CAMPSITE_MAP_ID, Campgrounds.modHelper.Translation.Get("messages.discovered.campsite", new
                {
                    campsiteName = Campgrounds.campManager.GetLocationNameFromDataId(campgroundData.Id)
                }));
            }
            @event.CurrentCommand++;
        }
    }
}
