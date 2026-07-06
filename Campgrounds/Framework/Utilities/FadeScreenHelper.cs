using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Utilities
{
    public static class FadeScreenHelper
    {
        public static bool IsFadingIn { get; private set; }
        public static bool IsFadingOut { get; private set; }

        public static bool IsFullyFadedIn { get { return IsFadingIn && _blackScreenAlpha >= 1f; } }

        private static float _blackScreenAlpha = 0f;

        private static bool _cachedHUDDisplaySetting = false;

        private static Action _afterFadeInAction;
        private static Action<SpriteBatch> _afterFullyFadedInAction;

        public static void Update()
        {
            if (IsFadingIn)
            {
                if (_blackScreenAlpha < 1f)
                {
                    _blackScreenAlpha += 0.01f;
                }
                else if (_afterFadeInAction is not null)
                {
                    _afterFadeInAction.Invoke();
                    _afterFadeInAction = null;
                }
            }
            else if (IsFadingOut)
            {
                if (_blackScreenAlpha > 0f)
                {
                    _blackScreenAlpha -= 0.01f;
                }
                else
                {
                    FinishedFadeOut();
                }
            }
        }

        public static void Draw(SpriteBatch b)
        {
            if (IsFadingIn || IsFadingOut)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * _blackScreenAlpha);
            }

            if (IsFullyFadedIn && _afterFullyFadedInAction is not null)
            {
                _afterFullyFadedInAction.Invoke(b);
            }
        }

        public static void StartFadeIn(Action afterFadeInAction = null, Action<SpriteBatch> afterFullyFadedInAction = null)
        {
            IsFadingIn = true;
            IsFadingOut = false;

            _blackScreenAlpha = 0f;
            _afterFadeInAction = afterFadeInAction;
            _afterFullyFadedInAction = afterFullyFadedInAction;

            _cachedHUDDisplaySetting = Game1.displayHUD;
            Game1.displayHUD = false;
            Game1.player.CanMove = false;
        }

        public static void StartFadeOut()
        {
            IsFadingIn = false;
            IsFadingOut = true;

            _blackScreenAlpha = 1f;
            _afterFullyFadedInAction = null;

            Game1.displayHUD = false;
            Game1.player.CanMove = false;
        }
        
        private static void FinishedFadeOut()
        {
            IsFadingOut = false;

            Game1.displayHUD = _cachedHUDDisplaySetting;
            Game1.player.CanMove = true;
        }

        public static void ImmediatelyStopFade()
        {
            IsFadingIn = false;
            IsFadingOut = false;

            _blackScreenAlpha = 0f;
            _afterFullyFadedInAction = null;

            Game1.displayHUD = _cachedHUDDisplaySetting;
            Game1.player.CanMove = true;
        }
    }
}
