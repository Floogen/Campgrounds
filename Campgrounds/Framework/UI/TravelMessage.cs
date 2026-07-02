using Campgrounds.Framework.Models;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace Campgrounds.Framework.UI
{
    public class TravelMessage
    {
        private CampgroundData _campgroundData;

        private string _travelMessageText = "";
        private double _travelMessageTimer = 3000;
        private double MESSAGE_FADE_START = 1500;

        private float _travelMessageAlpha = 1f;

        public TravelMessage(CampgroundData campgroundData)
        {
            _campgroundData = campgroundData;

            FadeScreenHelper.StartFadeIn(StartMessageDisplay, Draw);
        }

        private void StartMessageDisplay()
        {
            _travelMessageText = _campgroundData.TravelScreenText;
            if (string.IsNullOrEmpty(_travelMessageText) is true)
            {
                string travelTime = "quick";
                switch (_campgroundData.TravelTimeInHours)
                {
                    case > 2 and <= 4:
                        travelTime = "short";
                        break;
                    case > 4:
                        travelTime = "long";
                        break;
                }

                string travelType = "hike";
                if (_campgroundData.RequireVehicle)
                {
                    travelType = "drive";
                }

                _travelMessageText = $"After a {travelTime} {travelType}, you arrive at the campsite...";
            }
        }

        public void Update()
        {
            if (string.IsNullOrEmpty(_travelMessageText) is false)
            {
                _travelMessageTimer -= Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
                _travelMessageAlpha = MESSAGE_FADE_START <= 0f ? 0f : (float)Math.Clamp(_travelMessageTimer / MESSAGE_FADE_START, 0f, 1f);
            }

            if (_travelMessageTimer <= 0)
            {
                Campgrounds.campManager.StopTraveling();
                _travelMessageText = null;
                FadeScreenHelper.StartFadeOut();
            }
        }

        public void Draw(SpriteBatch b)
        {
            if (string.IsNullOrEmpty(_travelMessageText) is false)
            {
                var xOffset = (Game1.viewport.Width / 2) - StardewValley.BellsAndWhistles.SpriteText.getWidthOfString(_travelMessageText, 800) / 2;
                b.DrawString(Game1.dialogueFont, _travelMessageText, new Vector2(xOffset, Game1.viewport.Height / 2), Color.White * _travelMessageAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
        }
    }
}
