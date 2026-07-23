using Campgrounds.Framework.External.API;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Managers
{
    public class ApiManager : BaseManager
    {
        public IParchmentApi parchmentApi;

        public ApiManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            parchmentApi = helper.ModRegistry.GetApi<IParchmentApi>("PeacefulEnd.Parchment.Core");

            HandleGMCMApi(helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu"));
            HandleContentPatcherApi(helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher"));
        }

        private void HandleGMCMApi(IGenericModConfigMenuApi api)
        {
            if (api is null)
            {
                return;
            }

            api.Register(
                mod: Campgrounds.manifest,
                reset: () => Campgrounds.config = new Config(),
                save: () => helper.WriteConfig(Campgrounds.config)
            );

            api.AddKeybindList(
                mod: Campgrounds.manifest,
                name: () => "Open Camping Guide Shortcut",
                getValue: () => Campgrounds.config.GuideShortcut,
                setValue: value => Campgrounds.config.GuideShortcut = value
            );

            api.AddBoolOption(
                mod: Campgrounds.manifest,
                name: () => "Start Campgrounds Intro Immediately",
                tooltip: () => "Skips having to wait until at least Spring 14 to start Campgrounds. To start, go outside farmhouse to trigger the event.",
                getValue: () => Campgrounds.config.SkipCampgroundsIntroRequirement,
                setValue: value => Campgrounds.config.SkipCampgroundsIntroRequirement = value
            );
        }

        private void HandleContentPatcherApi(IContentPatcherAPI api)
        {
            if (api is null)
            {
                return;
            }

            api.RegisterToken(Campgrounds.manifest, "TotalNightsGoneCamping", () =>
            {
                if (Context.IsWorldReady)
                {
                    return new[] { Game1.stats.Get(Campgrounds.TOTAL_NIGHTS_GONE_CAMPING_STAT_ID).ToString() };
                }

                if (SaveGame.loaded?.player is not null)
                {
                    return new[] { SaveGame.loaded.player.stats.Get(Campgrounds.TOTAL_NIGHTS_GONE_CAMPING_STAT_ID).ToString() };
                }

                return null;
            });

            api.RegisterToken(Campgrounds.manifest, "TotalGuestsInvited", () =>
            {
                if (Context.IsWorldReady)
                {
                    return new[] { Game1.stats.Get(Campgrounds.TOTAL_GUESTS_INVITED_STAT_ID).ToString() };
                }

                if (SaveGame.loaded?.player is not null)
                {
                    return new[] { SaveGame.loaded.player.stats.Get(Campgrounds.TOTAL_GUESTS_INVITED_STAT_ID).ToString() };
                }

                return null;
            });

            api.RegisterToken(Campgrounds.manifest, "TotalCampMealsMade", () =>
            {
                if (Context.IsWorldReady)
                {
                    return new[] { Game1.stats.Get(Campgrounds.TOTAL_CAMP_MEALS_MADE_STAT_ID).ToString() };
                }

                if (SaveGame.loaded?.player is not null)
                {
                    return new[] { SaveGame.loaded.player.stats.Get(Campgrounds.TOTAL_CAMP_MEALS_MADE_STAT_ID).ToString() };
                }

                return null;
            });

            api.RegisterToken(Campgrounds.manifest, "TotalCampsitesUnlocked", () =>
            {
                if (Context.IsWorldReady)
                {
                    return new[] { Game1.stats.Get(Campgrounds.TOTAL_CAMPSITES_UNLOCKED_STAT_ID).ToString() };
                }

                if (SaveGame.loaded?.player is not null)
                {
                    return new[] { SaveGame.loaded.player.stats.Get(Campgrounds.TOTAL_CAMPSITES_UNLOCKED_STAT_ID).ToString() };
                }

                return null;
            });

            api.RegisterToken(Campgrounds.manifest, "TotalTentsUnlocked", () =>
            {
                if (Context.IsWorldReady)
                {
                    return new[] { Game1.stats.Get(Campgrounds.TOTAL_TENTS_UNLOCKED_STAT_ID).ToString() };
                }

                if (SaveGame.loaded?.player is not null)
                {
                    return new[] { SaveGame.loaded.player.stats.Get(Campgrounds.TOTAL_TENTS_UNLOCKED_STAT_ID).ToString() };
                }

                return null;
            });

            api.RegisterToken(Campgrounds.manifest, "TotalRecipesUnlocked", () =>
            {
                if (Context.IsWorldReady)
                {
                    return new[] { Game1.stats.Get(Campgrounds.TOTAL_RECIPES_UNLOCKED_STAT_ID).ToString() };
                }

                if (SaveGame.loaded?.player is not null)
                {
                    return new[] { SaveGame.loaded.player.stats.Get(Campgrounds.TOTAL_RECIPES_UNLOCKED_STAT_ID).ToString() };
                }

                return null;
            });
        }
    }
}
