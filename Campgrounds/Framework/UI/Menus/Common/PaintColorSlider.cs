using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Campgrounds.Framework.UI.Menus.Common
{
    public class PaintColorSlider
    {
        public const int DIVISIONS = 20;

        public ClickableTextureComponent handle;
        public TentPaintingMenu paintMenu;
        public Rectangle bounds;
        public int min;
        public int max;
        public Action<int> onValueSet;
        public Func<float, Color> getDrawColor;

        protected float _sliderPosition;
        protected int _displayedValue;

        public PaintColorSlider(TentPaintingMenu paintMenu, int handleId, Rectangle bounds, int min, int max, Action<int> onValueSet = null)
        {
            handle = new ClickableTextureComponent(new Rectangle(0, 0, 4, 5), Game1.mouseCursors, new Rectangle(72, 256, 16, 20), 1f)
            {
                myID = handleId,
                upNeighborID = TentPaintingMenu.SNAP_AUTOMATIC,
                upNeighborImmutable = true,
                downNeighborID = TentPaintingMenu.SNAP_AUTOMATIC,
                downNeighborImmutable = true,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            };
            this.paintMenu = paintMenu;
            this.bounds = bounds;
            this.min = min;
            this.max = max;
            this.onValueSet = onValueSet;
        }

        public virtual void ApplyMovementKey(int direction)
        {
            int amount = Math.Max((max - min) / 50, 1);
            if (direction == 3)
            {
                SetValue(_displayedValue - amount);
            }
            else
            {
                SetValue(_displayedValue + amount);
            }

            if (paintMenu.currentlySnappedComponent == handle && Game1.options.SnappyMenus)
            {
                paintMenu.snapCursorToCurrentSnappedComponent();
            }
        }

        public virtual void ReceiveLeftClick(int x, int y)
        {
            if (bounds.Contains(x, y))
            {
                paintMenu.activeSlider = this;
                SetValueFromPosition(x, y);
            }
        }

        public virtual void SetValueFromPosition(int x, int y)
        {
            if (bounds.Width == 0 || min == max)
            {
                return;
            }

            float newPosition = MathHelper.Clamp((x - bounds.Left) / (float)bounds.Width, 0f, 1f);
            if (_sliderPosition != newPosition)
            {
                _sliderPosition = newPosition;
                SetValue(min + (int)(_sliderPosition * (max - min)));
            }
        }

        public void SetValue(int value, bool skipValueSet = false)
        {
            value = Math.Clamp(value, min, max);
            _sliderPosition = (float)(value - min) / (max - min);
            handle.bounds.X = (int)Utility.Lerp(bounds.Left, bounds.Right, _sliderPosition) - handle.bounds.Width / 2 * 4;
            handle.bounds.Y = bounds.Top - 4;

            if (_displayedValue != value)
            {
                _displayedValue = value;
                if (!skipValueSet)
                {
                    onValueSet?.Invoke(value);
                }
            }
        }

        public int GetValue()
        {
            return _displayedValue;
        }

        public virtual void Draw(SpriteBatch b)
        {
            for (int i = 0; i < DIVISIONS; i++)
            {
                Rectangle sectionBounds = new Rectangle((int)(bounds.X + (float)bounds.Width / DIVISIONS * i), bounds.Y, (int)Math.Ceiling((float)bounds.Width / DIVISIONS), bounds.Height);
                Color drawnColor = getDrawColor?.Invoke(Utility.Lerp(min, max, (float)i / DIVISIONS)) ?? Color.Black;
                b.Draw(Game1.staminaRect, sectionBounds, drawnColor);
            }
            handle.draw(b);
        }

        public virtual void Update(int x, int y)
        {
            SetValueFromPosition(x, y);
        }
    }
}