using Campgrounds.Framework.Objects;
using Campgrounds.Framework.Patches;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace Campgrounds.Framework.Patches.Objects
{
    internal class ItemPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(Item);

        public ItemPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Item.actionWhenPurchased), new[] { typeof(string) }), postfix: new HarmonyMethod(GetType(), nameof(ActionWhenPurchasedPostfix)));
        }

        private static void ActionWhenPurchasedPostfix(Item __instance, string shopId)
        {
            if (__instance is null)
            {
                return;
            }

            if (Campgrounds.itemManager.IsCustomItem(__instance))
            {
                Campgrounds.itemManager.HandleCustomItem(Game1.player, __instance);
            }
        }
    }
}