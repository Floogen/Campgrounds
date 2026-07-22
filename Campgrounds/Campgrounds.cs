using Campgrounds.Framework;
using Campgrounds.Framework.Managers;
using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Data.Visitors;
using Campgrounds.Framework.Objects;
using Campgrounds.Framework.Patches.Characters;
using Campgrounds.Framework.Patches.Locations;
using Campgrounds.Framework.Patches.Objects;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;

namespace Campgrounds
{
    public class Campgrounds : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static IManifest manifest;
        internal static Multiplayer multiplayer;
        internal static Config config;

        internal static ApiManager apiManager;
        internal static CampingManager campManager;
        internal static CurrencyManager currencyManager;
        internal static ImmersionManager immersionManager;
        internal static ItemManager itemManager;
        internal static MessageManager messageManager;
        internal static TentManager tentManager;
        internal static VillagerManager villagerManager;
        internal static VisitorManager visitorManager;

        // Stats
        public static string TOTAL_NIGHTS_GONE_CAMPING_STAT_ID = "PeacefulEnd.Campgrounds_TotalNightsGoneCamping";
        public static string TOTAL_GUESTS_INVITED_STAT_ID = "PeacefulEnd.Campgrounds_TotalGuestsInvited";
        public static string TOTAL_CAMP_MEALS_MADE_STAT_ID = "PeacefulEnd.Campgrounds_TotalCampMealsMade";
        public static string TOTAL_CAMPSITES_UNLOCKED_STAT_ID = "PeacefulEnd.Campgrounds_TotalCampsitesUnlocked";
        public static string TOTAL_TENTS_UNLOCKED_STAT_ID = "PeacefulEnd.Campgrounds_TotalTentsUnlocked";
        public static string TOTAL_RECIPES_UNLOCKED_STAT_ID = "PeacefulEnd.Campgrounds_TotalRecipesUnlocked";

        // Paths
        public const string CAMPGROUND_DATA_PATH = "Data/PeacefulEnd.Campgrounds/Campgrounds";
        public const string CAMPING_TENTS_DATA_PATH = "Data/PeacefulEnd.Campgrounds/CampingTents";
        public const string CAMPFIRE_FOODS_DATA_PATH = "Data/PeacefulEnd.Campgrounds/CampfireFoods";
        public const string PARK_VISITORS_DATA_PATH = "Data/PeacefulEnd.Campgrounds/ParkVisitors";
        public const string VILLAGER_DATA_PATH = "Data/PeacefulEnd.Campgrounds/Villagers";

        public const string CAMPGROUND_DEFAULT_PREVIEW_TEXTURE_PATH = "Data/PeacefulEnd.Campgrounds/Campgrounds/Textures/Default_Preview";

        public const string CINDERSAP_PARK_MAP_PATH = "Maps/PeacefulEnd.Campgrounds.ContentPatcher_CindersapPark";
        public const string CINDERSAP_PARK_OVERGROWN_MAP_PATH = "Maps/PeacefulEnd.Campgrounds.ContentPatcher_CindersapParkOvergrown";
        public const string CINDERSAP_PARK_ACTIVE_MAP_PATH = "Maps/PeacefulEnd.Campgrounds.ContentPatcher_CindersapParkActive";

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            manifest = ModManifest;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            config = helper.ReadConfig<Config>();

            // Create managers
            apiManager = new ApiManager(monitor, modHelper);
            campManager = new CampingManager(monitor, helper);
            currencyManager = new CurrencyManager(monitor, helper);
            immersionManager = new ImmersionManager(monitor, helper);
            itemManager = new ItemManager(monitor, helper);
            messageManager = new MessageManager(monitor, helper);
            tentManager = new TentManager(monitor, helper);
            villagerManager = new VillagerManager(monitor, helper);
            visitorManager = new VisitorManager(monitor, helper);

            try
            {
                var harmony = new Harmony(ModManifest.UniqueID);

                // Apply Character patches
                new FarmerPatch(monitor, modHelper).Apply(harmony);
                new NPCPatch(monitor, modHelper).Apply(harmony);

                // Apply Location patches
                new GameLocationPatch(monitor, modHelper).Apply(harmony);

                // Apply Object patches
                new ObjectPatch(monitor, modHelper).Apply(harmony);
                new ItemPatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Hook into the required events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.OneSecondUpdateTicked += OneSecondUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.Rendered += OnRendered;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetsInvalidated += OnAssetInvalidated;

            // Register actions
            GameLocation.RegisterTileAction("PeacefulEnd.Campgrounds_CampingSiteList", MapActionHelper.HandleCampingSiteList);
            GameLocation.RegisterTileAction("PeacefulEnd.Campgrounds_CarRepair", MapActionHelper.HandleCarRepair);
            GameLocation.RegisterTileAction("PeacefulEnd.Campgrounds_RepairVisitorSite", MapActionHelper.HandleVisitorSiteRepair);
            GameLocation.RegisterTileAction("PeacefulEnd.Campgrounds_CampShopCounter", MapActionHelper.HandleCampShopCounter);
            GameLocation.RegisterTouchAction("PeacefulEnd.Campgrounds_ParkClosed", MapActionHelper.HandleParkClosed);
            GameLocation.RegisterTouchAction("PeacefulEnd.Campgrounds_CampingExit", (GameLocation location, string[] args, Farmer who, Vector2 tile) => MapActionHelper.HandleCampingExit(location, args, who, tile, false));

            // Register commands
            helper.ConsoleCommands.Add("campgrounds_addrations", "Adds camping ration currency to the farmer.", (cmd, args) => { if (int.TryParse(args[0], out int amount)) { currencyManager.ChangeCurrencyBalance(Framework.Models.Enums.Currency.CampRations, amount); } });
            helper.ConsoleCommands.Add("campgrounds_startcamp", "campgrounds_startcamp <CAMPGROUND_DATA_ID> [TENT_DATA_ID] [GUEST_NAME]", CampingHelper.StartCampingCommand);
            helper.ConsoleCommands.Add("campgrounds_setcampingtotaldays", "campgrounds_setcampingtotaldays <TOTAL_DAYS>", CampingHelper.SetTotalNightsCamping);
            helper.ConsoleCommands.Add("campgrounds_opententmenu", "campgrounds_opententmenu", (cmd, args) => { Game1.activeClickableMenu = new TentListMenu(); });

            // Register event command
            Event.RegisterCommand("PeacefulEnd.Campgrounds_GiveRations", EventHelper.GiveRationsCommand);
            Event.RegisterCommand("PeacefulEnd.Campgrounds_GiveCampsiteMap", EventHelper.GiveCampsiteMapCommand);
            Event.RegisterCommand("PeacefulEnd.Campgrounds_GiveCampfireRecipe", EventHelper.GiveCampfireRecipeCommand);
            Event.RegisterCommand("PeacefulEnd.Campgrounds_GiveTentSchematic", EventHelper.GiveTentSchematicCommand);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Handle custom items
            ItemQueryResolver.Register(CurrencyManager.CAMP_RATION_CURRENCY_ID, (string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError) => {
                return new[]
                {
                    new ItemQueryResult(new CampRations())
                };
            });

            ItemQueryResolver.Register(ItemManager.CAMPSITE_MAP_ID, (string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError) => {
                string[] args = ArgUtility.SplitBySpaceQuoteAware(arguments);
                if (ArgUtility.TryGet(args, 0, out string campgroundId, out string error) is false)
                {
                    return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error);
                }

                var campgroundData = campManager.CampgroundData.FirstOrDefault(c => c.Id.EqualsIgnoreCase(campgroundId));
                if (campgroundData is null)
                {
                    return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, $"No campground found with the ID \"{campgroundId}\".");
                }

                Item item = ItemRegistry.Create(ItemManager.CAMPSITE_MAP_ID);
                item.modData[ItemManager.CAMPSITE_MAP_MOD_DATA_ID] = campgroundData.Id;

                return new[] { new ItemQueryResult(item) };
            });

            ItemQueryResolver.Register(ItemManager.CAMPFIRE_RECIPE_ID, (string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError) => {
                string[] args = ArgUtility.SplitBySpaceQuoteAware(arguments);
                if (ArgUtility.TryGet(args, 0, out string campfireFoodDataId, out string error) is false)
                {
                    return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error);
                }

                var campfireFoodData = campManager.GetCampfireFoodDataById(campfireFoodDataId);
                if (campfireFoodData is null)
                {
                    return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, $"No campfire food found with the ID \"{campfireFoodDataId}\".");
                }

                Item item = ItemRegistry.Create(ItemManager.CAMPFIRE_RECIPE_ID);
                item.modData[ItemManager.CAMPFIRE_RECIPE_MOD_DATA_ID] = campfireFoodData.Id;

                return new[] { new ItemQueryResult(item) };
            });

            ItemQueryResolver.Register(ItemManager.TENT_SCHEMATIC_ID, (string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError) => {
                string[] args = ArgUtility.SplitBySpaceQuoteAware(arguments);
                if (ArgUtility.TryGet(args, 0, out string campingTentDataId, out string error) is false)
                {
                    return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, error);
                }

                var campingTentData = tentManager.GetTentDataById(campingTentDataId);
                if (campingTentData is null)
                {
                    return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, $"No camping tent found with the ID \"{campingTentDataId}\".");
                }

                Item item = ItemRegistry.Create(ItemManager.TENT_SCHEMATIC_ID);
                item.modData[ItemManager.TENT_SCHEMATIC_MOD_DATA_ID] = campingTentData.Id;

                return new[] { new ItemQueryResult(item) };
            });

            // Handle custom visitor shops
            ItemQueryResolver.Register($"PeacefulEnd.Campgrounds_UNKNOWN_COOKING_RECIPES", (string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError) => {
                Farmer who = context.Player ?? Game1.player;

                var items = new List<ItemQueryResult>(); 
                foreach (var recipeObject in ShopHelper.GetDailyUnknownCookingRecipes(who, int.MaxValue))
                {
                    items.Add(new ItemQueryResult(recipeObject));
                }

                return items;
            });
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            FadeScreenHelper.Update();
        }

        private void OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            // Update various stats that don't trigger counts on their own
            Game1.stats.Set(TOTAL_CAMPSITES_UNLOCKED_STAT_ID, campManager.CampgroundData.Count(c => c.IsUnlocked()));
            Game1.stats.Set(TOTAL_TENTS_UNLOCKED_STAT_ID, tentManager.CampingTentData.Count(c => c.IsUnlocked()));
            Game1.stats.Set(TOTAL_RECIPES_UNLOCKED_STAT_ID, campManager.CampfireFoodData.Count(c => c.IsUnlocked()));
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // Repair garage car (if needed)
            var garageLocation = Game1.getLocationFromName("PeacefulEnd.Campgrounds.ContentPatcher_CindersapParkGarage");
            if (garageLocation is not null)
            {
                CampingHelper.AttemptRepairCarMapTiles(garageLocation);
            }

            // Clear bushes in front of park entrance
            var forestLocation = Game1.getLocationFromName("Forest");
            if (forestLocation is not null)
            {
                var boundary = new Rectangle(8, 0, 5, 12);
                var removedBushes = forestLocation.largeTerrainFeatures.RemoveWhere(t => t is Bush && boundary.Contains(t.Tile));

                if (removedBushes > 0)
                {
                    Monitor.Log($"Removed {removedBushes} bushes that were in the way of park entrance.", LogLevel.Info);
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsUseToolButton() && Context.IsPlayerFree && Game1.activeClickableMenu is null && Game1.eventUp is false && Game1.player.CanMove is true && Game1.player.CurrentTool?.ItemId == "PeacefulEnd.Campgrounds.Tools.WalkieTalkie")
            {
                // Consume the button press to prevent the default animation from playing
                Helper.Input.Suppress(e.Button);

                // Ask if they want to start the cutscene (to show who is at park)
                string text = Helper.Translation.Get(Game1.timeOfDay >= 1200 ? "questions.tools.walkieTalkie.afternoon" : "questions.tools.walkieTalkie.morning", new { playerName = Game1.player.displayName });
                Game1.currentLocation.createQuestionDialogue(text, Game1.currentLocation.createYesNoResponses(), CampingHelper.OnWalkieTalkieCheckParkResponse, null);
            }

            if (config.GuideShortcut is not null && config.GuideShortcut.IsDown() && apiManager.parchmentApi is not null && Context.IsPlayerFree && Game1.activeClickableMenu is null)
            {
                apiManager.parchmentApi.TryOpenBook("PeacefulEnd.Campgrounds.Parchment_CampingGuide");
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
            else if (e.NameWithoutLocale.IsEquivalentTo(PARK_VISITORS_DATA_PATH))
            {
                e.LoadFrom(() => visitorManager.VisitorData, AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(CAMPGROUND_DEFAULT_PREVIEW_TEXTURE_PATH))
            {
                e.LoadFrom(() => Helper.ModContent.Load<Texture2D>("Framework/Assets/defaultCampgroundPreview.png"), AssetLoadPriority.Medium);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(CINDERSAP_PARK_MAP_PATH))
            {
                e.Edit(asset =>
                {
                    // Load the source map
                    Map source = Helper.GameContent.Load<Map>(CINDERSAP_PARK_OVERGROWN_MAP_PATH);

                    // Check which visitor campsite(s) have been unlocked
                    var editor = asset.AsMap();
                    if (NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(1)) is false)
                    {
                        editor.PatchMap(
                            source: source,
                            sourceArea: new Rectangle(0, 25, 22, 26),
                            targetArea: new Rectangle(0, 25, 22, 26),
                            patchMode: PatchMapMode.Replace
                        );
                    }
                    if (NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(2)) is false)
                    {
                        editor.PatchMap(
                            source: source,
                            sourceArea: new Rectangle(0, 0, 35, 25),
                            targetArea: new Rectangle(0, 0, 35, 25),
                            patchMode: PatchMapMode.Replace
                        );
                    }
                    if (NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(3)) is false)
                    {
                        editor.PatchMap(
                            source: source,
                            sourceArea: new Rectangle(40, 0, 29, 51),
                            targetArea: new Rectangle(40, 0, 29, 51),
                            patchMode: PatchMapMode.Replace
                        );
                    }
                });
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

            var visitorData = e.NamesWithoutLocale.FirstOrDefault(a => a.IsEquivalentTo(PARK_VISITORS_DATA_PATH));
            if (visitorData is not null)
            {
                visitorManager.VisitorData = Helper.GameContent.Load<List<VisitorData>>(PARK_VISITORS_DATA_PATH);
            }
        }
    }
}
