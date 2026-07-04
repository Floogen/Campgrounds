using Campgrounds.Framework.Managers;
using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Crops;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Campgrounds
{
    public class Campgrounds : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;

        internal static CampingManager campManager;

        public const string CAMPGROUND_DATA_PATH = "Data/PeacefulEnd_Campgrounds/Campgrounds";
        public const string CAMPING_TENTS_DATA_PATH = "Data/PeacefulEnd_Campgrounds/CampingTents";
        public const string CAMPGROUND_DEFAULT_PREVIEW_TEXTURE_PATH = "Data/PeacefulEnd_Campgrounds/Campgrounds/Textures/Default_Preview";

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Create managers
            campManager = new CampingManager(monitor, helper);

            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Add patches here
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Hook into the required events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.Rendered += OnRendered;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;

            // Register actions
            GameLocation.RegisterTileAction("PeacefulEnd.Campgrounds_CampingSiteList", MapActionHelper.HandleCampingSiteList);
            GameLocation.RegisterTouchAction("PeacefulEnd.Campgrounds_CampingExit", MapActionHelper.HandleCampingExit);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {

        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            FadeScreenHelper.Update();
        }

        private void OnRendered(object sender, RenderedEventArgs e)
        {
            FadeScreenHelper.Draw(e.SpriteBatch);
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"Data/PeacefulEnd_Campgrounds/Villagers"))
            {
                //e.LoadFrom(() => textureManager.GetIdToAppearanceModels<BodyContentPack>(), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(CAMPGROUND_DATA_PATH))
            {
                e.LoadFrom(() => campManager.CampgroundData, AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(CAMPING_TENTS_DATA_PATH))
            {
                e.LoadFrom(() => campManager.CampingTentData, AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(CAMPGROUND_DEFAULT_PREVIEW_TEXTURE_PATH))
            {
                e.LoadFrom(() => Helper.ModContent.Load<Texture2D>("Framework/Assets/defaultCampgroundPreview.png"), AssetLoadPriority.Medium);
            }
        }

        private void OnAssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            var villagerData = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo("Data/PeacefulEnd_Campgrounds/Villagers"));
            if (villagerData is not null)
            {
                //textureManager.Sync(Helper.GameContent.Load<Dictionary<string, AppearanceContentPack>>(appearanceDataAsset));
            }

            var campData = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo(CAMPGROUND_DATA_PATH));
            if (campData is not null)
            {
                campManager.CampgroundData = Helper.GameContent.Load<List<CampgroundData>>(CAMPGROUND_DATA_PATH);
            }

            var campingTentsData = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo(CAMPING_TENTS_DATA_PATH));
            if (campingTentsData is not null)
            {
                campManager.CampingTentData = Helper.GameContent.Load<List<CampingTentData>>(CAMPING_TENTS_DATA_PATH);
            }
        }
    }
}
