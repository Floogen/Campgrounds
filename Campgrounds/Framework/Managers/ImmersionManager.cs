using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using Microsoft.Xna.Framework;
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
    public class ImmersionManager : BaseManager
    {
        public const string CAVE_DRIP_EFFECT_FIELD_ID = "CAVE_DRIP_EFFECT_FIELD_ID";
        private const double DEFAULT_CAVE_DRIP_PER_SECOND = 0.12;
        private const int DRIP_SPREAD_TILES = 8;

        public ImmersionManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
        }

        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation is null)
            {
                return;
            }

            var data = Game1.currentLocation.GetData();
            if (data is null || data.CustomFields is null || data.CustomFields.TryGetValue(CAVE_DRIP_EFFECT_FIELD_ID, out string givenCaveDripPerSecond) is false)
            {
                return;
            }

            var dripChancePerSecond = DEFAULT_CAVE_DRIP_PER_SECOND;
            if (double.TryParse(givenCaveDripPerSecond, out dripChancePerSecond) is false || Game1.random.NextDouble() >= dripChancePerSecond)
            {
                return;
            }

            Vector2 dripPosition = GetRandomTileNearPlayer();
            Game1.currentLocation.playSound("cavedrip", dripPosition);
        }

        private Vector2 GetRandomTileNearPlayer()
        {
            int offsetX = Game1.random.Next(-DRIP_SPREAD_TILES, DRIP_SPREAD_TILES + 1);
            int offsetY = Game1.random.Next(-DRIP_SPREAD_TILES, DRIP_SPREAD_TILES + 1);

            return Game1.player.Tile + new Vector2(offsetX, offsetY);
        }
    }
}
