using Campgrounds.Framework.Patches.Locations;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;

namespace Campgrounds
{
    public class Campgrounds : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Location patches
                new GameLocationPatch(monitor, helper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Hook into the required events
            GameLocation.RegisterTouchAction("PeacefulEnd.Campgrounds_CampingExit", MapActionHelper.HandleCampingExit);
        }
    }
}
