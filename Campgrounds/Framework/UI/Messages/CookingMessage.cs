using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Campgrounds.Framework.UI.Messages
{
    public class CookingMessage : Message
    {
        private string _messageText = "";
        private string _secondaryMessageText = "";
        private double _messageTimer = 2000;
        private double MESSAGE_FADE_START = 1000;
        private bool _hasDisplayedSecondaryMessage = false;

        private float _travelMessageAlpha = 1f;
        private float _secondaryMessageAlpha = 0f;

        public CookingMessage()
        {
            FadeScreenHelper.StartFadeIn(PlayCinematic, Draw);
        }

        private void PlayCinematic()
        {
            _messageText = Campgrounds.modHelper.Translation.Get("messages.cooking.fluff1");

            Game1.playSound("fireball");
            DelayedAction.playSoundAfterDelay("bubbles", 1000);
            DelayedAction.playSoundAfterDelay("eat", 2500);
            DelayedAction.playSoundAfterDelay("gulp", 3500);
        }

        public override bool Update()
        {
            if (string.IsNullOrEmpty(_messageText) is false)
            {
                _messageTimer -= Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;

                if (string.IsNullOrEmpty(_secondaryMessageText) is false)
                {
                    _travelMessageAlpha = MESSAGE_FADE_START <= 0f ? 0f : (float)Math.Clamp(_messageTimer / MESSAGE_FADE_START, 0f, 1f);

                    _secondaryMessageAlpha = _travelMessageAlpha;
                    if (MESSAGE_FADE_START <= _messageTimer)
                    {
                        _secondaryMessageAlpha = (float)Math.Clamp((5000 - _messageTimer) / 2000, 0f, 1f);
                    }
                }
            }

            if (_messageTimer <= 0)
            {
                Game1.musicPlayerVolume = Game1.options.musicVolumeLevel * 0.3f;
                Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);

                if (_hasDisplayedSecondaryMessage)
                {
                    _messageText = null;
                    _secondaryMessageText = null;
                    FadeScreenHelper.StartFadeOut();

                    return false;
                }
                else
                {
                    _secondaryMessageText = Campgrounds.modHelper.Translation.Get("messages.cooking.fluff2");
                    _messageTimer = 5000;
                    _hasDisplayedSecondaryMessage = true;
                }
            }
            else
            {
                Game1.player.CanMove = false;
            }

            return true;
        }

        public override void Draw(SpriteBatch b)
        {
            if (string.IsNullOrEmpty(_messageText) is false)
            {
                Vector2 textSize = Game1.dialogueFont.MeasureString(_messageText);
                Vector2 position = new Vector2((Game1.uiViewport.Width - textSize.X) / 2, (Game1.uiViewport.Height - textSize.Y) / 2);
                b.DrawString(Game1.dialogueFont, _messageText, position - new Vector2(0, 60), Color.White * _travelMessageAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            if (string.IsNullOrEmpty(_secondaryMessageText) is false)
            {
                Vector2 textSize = Game1.dialogueFont.MeasureString(_secondaryMessageText);
                Vector2 position = new Vector2((Game1.uiViewport.Width - textSize.X) / 2, (Game1.uiViewport.Height - textSize.Y) / 2);
                b.DrawString(Game1.dialogueFont, _secondaryMessageText, position + new Vector2(0, 60), Color.White * _secondaryMessageAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
        }
    }
}
