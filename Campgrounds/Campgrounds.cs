using Campgrounds.Framework.Managers;
using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Objects;
using Campgrounds.Framework.Patches.Characters;
using Campgrounds.Framework.Patches.Locations;
using Campgrounds.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Internal;
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
        internal static CurrencyManager currencyManager;
        internal static MessageManager messageManager;
        internal static TentManager tentManager;
        internal static VillagerManager villagerManager;

        public const string CAMPGROUND_DATA_PATH = "Data/PeacefulEnd_Campgrounds/Campgrounds";
        public const string CAMPING_TENTS_DATA_PATH = "Data/PeacefulEnd_Campgrounds/CampingTents";
        public const string CAMPFIRE_FOODS_DATA_PATH = "Data/PeacefulEnd_Campgrounds/CampfireFoods";
        public const string VILLAGER_DATA_PATH = "Data/PeacefulEnd_Campgrounds/Villagers";

        public const string CAMPGROUND_DEFAULT_PREVIEW_TEXTURE_PATH = "Data/PeacefulEnd_Campgrounds/Campgrounds/Textures/Default_Preview";

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Create managers
            campManager = new CampingManager(monitor, helper);
            currencyManager = new CurrencyManager(monitor, helper);
            messageManager = new MessageManager(monitor, helper);
            tentManager = new TentManager(monitor, helper);
            villagerManager = new VillagerManager(monitor, helper);

            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                // Apply Character patches
                new FarmerPatch(monitor, modHelper).Apply(harmony);
                new NPCPatch(monitor, modHelper).Apply(harmony);

                // Apply Location patches
                new GameLocationPatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Hook into the required events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.Display.Rendered += OnRendered;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;

            // Register actions
            GameLocation.RegisterTileAction("PeacefulEnd.Campgrounds_CampShop", MapActionHelper.HandleCampShop);
            GameLocation.RegisterTileAction("PeacefulEnd.Campgrounds_CampingSiteList", MapActionHelper.HandleCampingSiteList);
            GameLocation.RegisterTileAction("PeacefulEnd.Campgrounds_CarRepair", MapActionHelper.HandleCarRepair);
            GameLocation.RegisterTouchAction("PeacefulEnd.Campgrounds_CampingExit", (GameLocation location, string[] args, Farmer who, Vector2 tile) => MapActionHelper.HandleCampingExit(location, args, who, tile, false));

            // Register commands
            helper.ConsoleCommands.Add("campgrounds_addrations", "Adds camping ration currency to the farmer.", (cmd, args) => { if (int.TryParse(args[0], out int amount)) { currencyManager.ChangeCurrencyBalance(Framework.Models.Enums.Currency.CampRations, amount); } });
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ItemQueryResolver.Register(CurrencyManager.CAMP_RATION_CURRENCY_ID, (string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError) => {
                return new[]
                {
                    new ItemQueryResult(new CampRations())
                };
            });

            ItemQueryResolver.Register(CampsiteMap.CAMPSITE_MAP_ID, (string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError) => {
                string[] args = ArgUtility.SplitBySpaceQuoteAware(arguments);
                if (ArgUtility.TryGet(args, 0, out string campgroundId, out string error) is false || (campManager.CampgroundData.FirstOrDefault(c => c.Id.EqualsIgnoreCase(campgroundId)) is var campgroundData && campgroundData is null))
                {
                    return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error);
                }

                return new[]
                {
                    new ItemQueryResult(new CampsiteMap(campgroundData))
                };
            });
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            FadeScreenHelper.Update();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            campManager.FindActiveCampsites();

            // Repair garage car (if needed)
            var garageLocation = Game1.getLocationFromName("PeacefulEnd.Campgrounds.ContentPatcher_CindersapParkGarage");
            if (garageLocation is not null)
            {
                CampingHelper.AttemptRepairCarMapTiles(garageLocation);
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            // Sanitize any campsites before saving to prevent serialization issues
            foreach (var campsite in campManager.ActiveCampsites)
            {
                campsite.Sanitize();
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            campManager.HandleActiveCampsites();
            campManager.HandleForageSpawning();
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            var campsite = campManager.GetActiveCampsiteFromLocation(e.OldLocation);
            if (campsite is not null)
            {
                campManager.EndCampingTrip(e.OldLocation);
            }
        }

        private void OnRendered(object sender, RenderedEventArgs e)
        {
            FadeScreenHelper.Draw(e.SpriteBatch);
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(CAMPGROUND_DATA_PATH))
            {
                e.LoadFrom(() => campManager.CampgroundData, AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(CAMPFIRE_FOODS_DATA_PATH))
            {
                e.LoadFrom(() => campManager.CampfireFoodData, AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(CAMPING_TENTS_DATA_PATH))
            {
                e.LoadFrom(() => tentManager.CampingTentData, AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(VILLAGER_DATA_PATH))
            {
                e.LoadFrom(() => villagerManager.VillagerData, AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(CAMPGROUND_DEFAULT_PREVIEW_TEXTURE_PATH))
            {
                e.LoadFrom(() => Helper.ModContent.Load<Texture2D>("Framework/Assets/defaultCampgroundPreview.png"), AssetLoadPriority.Medium);
            }
        }

        private void OnAssetInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            var campData = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo(CAMPGROUND_DATA_PATH));
            if (campData is not null)
            {
                campManager.CampgroundData = Helper.GameContent.Load<List<CampgroundData>>(CAMPGROUND_DATA_PATH);
            }

            var campfireFoodsData = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo(CAMPFIRE_FOODS_DATA_PATH));
            if (campfireFoodsData is not null)
            {
                campManager.CampfireFoodData = Helper.GameContent.Load<List<CampfireFoodData>>(CAMPFIRE_FOODS_DATA_PATH);
            }

            var campingTentsData = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo(CAMPING_TENTS_DATA_PATH));
            if (campingTentsData is not null)
            {
                tentManager.CampingTentData = Helper.GameContent.Load<List<CampingTentData>>(CAMPING_TENTS_DATA_PATH);
            }

            var villagerData = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo(VILLAGER_DATA_PATH));
            if (villagerData is not null)
            {
                villagerManager.VillagerData = Helper.GameContent.Load<List<VillagerData>>(VILLAGER_DATA_PATH);
            }
        }
    }
}
