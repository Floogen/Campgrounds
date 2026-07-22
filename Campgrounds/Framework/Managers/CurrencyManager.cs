using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
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
    public class CurrencyManager : BaseManager
    {
        public const string CAMP_RATION_CURRENCY_ID = "PeacefulEnd.Campgrounds.Currency.CampRations.Id";
        public const string CAMP_RATION_BALANCE_ID = "PeacefulEnd.Campgrounds.Currency.CampRations.Balance";

        private readonly NetIntDelta _campRations = new NetIntDelta() { Minimum = 0 };
        private double _shakeTimer;

        public CurrencyManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            SaveCurrencyBalance(Currency.CampRations);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            _campRations.Value = RestoreCurrencyBalance(Currency.CampRations);

            // Register the currency display
            Game1.specialCurrencyDisplay.Register(CAMP_RATION_CURRENCY_ID, _campRations, drawIcon: DrawIcon);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (_shakeTimer > 0)
            {
                _shakeTimer -= Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
            }
        }

        public void ShowCurrency(Currency currency, Func<bool> keepOpen = null, float timeToLive = 5f)
        {
            switch (currency)
            {
                case Currency.CampRations:
                    Game1.specialCurrencyDisplay.ShowCurrency(CAMP_RATION_CURRENCY_ID, keepOpen, timeToLive);
                    break;
            }
        }

        public int GetCurrencyBalance(Currency currency)
        {
            switch (currency)
            {
                case Currency.CampRations:
                    return _campRations.Value;
            }
            return 0;
        }

        public bool ChangeCurrencyBalance(Currency currency, int amount)
        {
            switch (currency)
            {
                case Currency.CampRations:
                    if (amount < 0 && _campRations.Value + amount < 0)
                    {
                        return false;
                    }

                    _campRations.Value += amount;
                    return true;
            }

            return false;
        }

        private int RestoreCurrencyBalance(Currency currency)
        {
            switch (currency)
            {
                case Currency.CampRations:
                    return Game1.player.modData.TryGetValue(CAMP_RATION_BALANCE_ID, out string rawBalance) && int.TryParse(rawBalance, out int value) ? value : 0;
            }

            return 0;
        }

        private void SaveCurrencyBalance(Currency currency)
        {
            switch (currency)
            {
                case Currency.CampRations:
                    Game1.player.modData[CAMP_RATION_BALANCE_ID] = _campRations.ToString();
                    break;
            }
        }

        public void ShakeCurrencyIcon(double durationInMilliseconds)
        {
            _shakeTimer = durationInMilliseconds;
        }

        private void DrawIcon(SpriteBatch b, Vector2 position)
        {
            if (_shakeTimer > 0)
            {
                position += 1f * new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
            }

            b.Draw(GetTexture(), position, GetSourceRectangle(Currency.CampRations), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
        }

        public Texture2D GetTexture()
        {
            return helper.GameContent.Load<Texture2D>("Data/PeacefulEnd.Campgrounds/Campgrounds/Textures/CurrencyIcons");
        }

        public Rectangle GetSourceRectangle(Currency currency)
        {
            var sourceRectangle = new Rectangle(0, 0, 16, 16);
            switch (currency)
            {
                case Currency.CampRations:
                    sourceRectangle.X = 0;
                    sourceRectangle.Y = 0;
                    break;
            }

            return sourceRectangle;
        }
    }
}
