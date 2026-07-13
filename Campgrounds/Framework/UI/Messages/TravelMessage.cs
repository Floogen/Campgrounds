using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace Campgrounds.Framework.UI.Messages
{
    public class TravelMessage : Message
    {
        private CampgroundData _campgroundData;

        private string _travelMessageText = "";
        private double _travelMessageTimer = 4000;
        private double MESSAGE_FADE_START = 2000;

        private float _travelMessageAlpha = 1f;

        public TravelMessage(CampgroundData campgroundData)
        {
            _campgroundData = campgroundData;

            FadeScreenHelper.StartFadeIn(WarpAndDisplayMessage, Draw);
        }

        private void WarpAndDisplayMessage()
        {
            // Warp and skip the vanilla' fade to black logic
            Game1.warpFarmer(_campgroundData.Id, (int)_campgroundData.PlayerSpawnTile.Value.X, (int)_campgroundData.PlayerSpawnTile.Value.Y, 2);
            Game1.fadeToBlackAlpha = 1.2f;

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

                string travelType = _campgroundData.RequireVehicle ? "drive" : "hike";

                bool hasFriend = Campgrounds.villagerManager.GetInvitedCharacter(Game1.player) != null;
                string baseKey = hasFriend ? "travel.arrivalWithFriend" : "travel.arrival";

                _travelMessageText = Campgrounds.modHelper.Translation.Get($"{baseKey}.{travelType}.{travelTime}");
            }
        }

        public override bool Update()
        {
            if (string.IsNullOrEmpty(_travelMessageText) is false)
            {
                _travelMessageTimer -= Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
                _travelMessageAlpha = MESSAGE_FADE_START <= 0f ? 0f : (float)Math.Clamp(_travelMessageTimer / MESSAGE_FADE_START, 0f, 1f);
            }

            if (_travelMessageTimer <= 0)
            {
                _travelMessageText = null;
                FadeScreenHelper.StartFadeOut();

                return false;
            }
            else
            {
                Game1.player.CanMove = false;
            }

            return true;
        }

        public override void Draw(SpriteBatch b)
        {
            if (string.IsNullOrEmpty(_travelMessageText) is false)
            {
                Vector2 textSize = Game1.dialogueFont.MeasureString(_travelMessageText);
                Vector2 position = new Vector2((Game1.uiViewport.Width - textSize.X) / 2, (Game1.uiViewport.Height - textSize.Y) / 2);
                b.DrawString(Game1.dialogueFont, _travelMessageText, position, Color.White * _travelMessageAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
        }
    }
}
