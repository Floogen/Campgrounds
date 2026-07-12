using Campgrounds.Framework.Objects;
using Campgrounds.Framework.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace Campgrounds.Framework.Patches.Objects
{
    internal class ObjectPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(Object);

        public ObjectPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "get_DisplayName", null), postfix: new HarmonyMethod(GetType(), nameof(GetNamePostfix)));
            harmony.Patch(AccessTools.Method(_object, "getDescription", null), postfix: new HarmonyMethod(GetType(), nameof(GetDescriptionPostfix)));
        }

        private static void GetNamePostfix(Object __instance, ref string __result)
        {
            if (Campgrounds.itemManager.HasCustomName(__instance) is var customName && customName.Result is true)
            {
                __result = customName.Name;
                return;
            }
        }

        private static void GetDescriptionPostfix(Object __instance, ref string __result)
        {
            if (Campgrounds.itemManager.HasCustomDescription(__instance) is var customDescription && customDescription.Result is true)
            {
                __result = customDescription.Description;
                return;
            }
        }
    }
}