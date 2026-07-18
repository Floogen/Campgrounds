using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.UI.Menus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace Campgrounds.Framework.UI.Menus
{
    public class TentPaintingMenu : IClickableMenu
    {
        public const int REGION_COLOR_BUTTONS = 1000;
        public const int REGION_OK_BUTTON = 101;
        public const int REGION_COPY_COLOR = 104;
        public const int REGION_DEFAULT_COLOR = 105;
        public const int REGION_HUE_SLIDER = 106;
        public const int REGION_SATURATION_SLIDER = 107;
        public const int REGION_LIGHTNESS_SLIDER = 108;

        public const int SNAP_AUTOMATIC = -99998;
        public const int WINDOW_WIDTH = 1024;
        public const int WINDOW_HEIGHT = 576;
        public const int PREVIEW_PANE_WIDTH = 512;
        public const int COLOR_PANE_WIDTH = 448;
        public const int MAX_SAVED_COLORS = 8;
        public const int SLIDER_ROW_HEIGHT = 24;
        public const float PREVIEW_SCALE = 4f;

        public static List<Vector3> savedColors = new List<Vector3>();

        private CampingTentData _campingTentData;

        public Texture2D baseTexture;
        public Texture2D recolorTexture;
        public Rectangle baseSourceRect;
        public Rectangle recolorSourceRect;
        public string title;
        public PaintColor colorTarget;
        public Action<PaintColor> onColorChanged;

        public Rectangle previewPane;
        public Rectangle colorPane;
        public Vector2 colorDrawPosition;

        public PaintColorSlider activeSlider;
        public PaintColorSlider hueSlider;
        public PaintColorSlider saturationSlider;
        public PaintColorSlider lightnessSlider;

        public ClickableTextureComponent okButton;
        public ClickableTextureComponent copyColorButton;
        public ClickableTextureComponent defaultColorButton;
        public List<ClickableTextureComponent> savedColorButtons = new List<ClickableTextureComponent>();

        public List<ClickableComponent> sliderHandles = new List<ClickableComponent>();

        public List<Color> buttonColors = new List<Color>();
        private string hoverText = "";

        public TentPaintingMenu(CampingTentData campingTentData, PaintColor colorTarget, string title = null) : base(Game1.uiViewport.Width / 2 - WINDOW_WIDTH / 2, Game1.uiViewport.Height / 2 - WINDOW_HEIGHT / 2, WINDOW_WIDTH, WINDOW_HEIGHT)
        {
            Game1.player.Halt();

            _campingTentData = campingTentData;

            baseTexture = Campgrounds.modHelper.GameContent.Load<Texture2D>(_campingTentData.TexturePath);
            recolorTexture = Campgrounds.modHelper.GameContent.Load<Texture2D>(_campingTentData.GrayscaleTexturePath);

            this.colorTarget = colorTarget;
            this.title = title;

            this.baseSourceRect = _campingTentData.SouthSprite.DisplayRectangle;
            this.recolorSourceRect = _campingTentData.SouthSprite.DisplayRectangle;

            RepositionElements();

            if (Game1.options.SnappyMenus)
            {
                snapToDefaultClickableComponent();
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = getComponentWithID(REGION_OK_BUTTON);
            snapCursorToCurrentSnappedComponent();
        }

        public virtual void RepositionElements()
        {
            previewPane = new Rectangle(xPositionOnScreen, yPositionOnScreen, PREVIEW_PANE_WIDTH, WINDOW_HEIGHT);
            colorPane = new Rectangle(xPositionOnScreen + width - COLOR_PANE_WIDTH, yPositionOnScreen, COLOR_PANE_WIDTH, WINDOW_HEIGHT);

            Rectangle panelRectangle = colorPane;
            panelRectangle.Inflate(-32, -32);
            panelRectangle.Y += 64;
            panelRectangle.Height = 0;

            int colorX = panelRectangle.Left;
            defaultColorButton = new ClickableTextureComponent(new Rectangle(colorX, panelRectangle.Bottom, 64, 64), Game1.mouseCursors2, new Rectangle(80, 144, 16, 16), 4f)
            {
                region = REGION_COLOR_BUTTONS,
                myID = REGION_DEFAULT_COLOR,
                upNeighborID = SNAP_AUTOMATIC,
                downNeighborID = SNAP_AUTOMATIC,
                leftNeighborID = SNAP_AUTOMATIC,
                rightNeighborID = SNAP_AUTOMATIC,
                fullyImmutable = true
            };
            colorX += 80;

            savedColorButtons.Clear();
            buttonColors.Clear();
            for (int i = 0; i < savedColors.Count; i++)
            {
                if (colorX + 64 > panelRectangle.Right)
                {
                    colorX = panelRectangle.X;
                    panelRectangle.Y += 72;
                }

                savedColorButtons.Add(new ClickableTextureComponent(new Rectangle(colorX, panelRectangle.Bottom, 64, 64), Game1.mouseCursors2, new Rectangle(96, 144, 16, 16), 4f)
                {
                    region = REGION_COLOR_BUTTONS,
                    myID = i,
                    upNeighborID = SNAP_AUTOMATIC,
                    downNeighborID = SNAP_AUTOMATIC,
                    leftNeighborID = SNAP_AUTOMATIC,
                    rightNeighborID = SNAP_AUTOMATIC,
                    fullyImmutable = true
                });
                colorX += 80;

                Vector3 savedColor = savedColors[i];
                Utility.HSLtoRGB(savedColor.X, savedColor.Y / 100f, Utility.Lerp(0.25f, 0.5f, savedColor.Z), out var red, out var green, out var blue);
                buttonColors.Add(new Color((byte)red, (byte)green, (byte)blue));
            }

            if (colorX + 64 > panelRectangle.Right)
            {
                colorX = panelRectangle.X;
                panelRectangle.Y += 72;
            }
            copyColorButton = new ClickableTextureComponent(new Rectangle(colorX, panelRectangle.Bottom, 64, 64), Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f)
            {
                region = REGION_COLOR_BUTTONS,
                myID = REGION_COPY_COLOR,
                upNeighborID = SNAP_AUTOMATIC,
                downNeighborID = SNAP_AUTOMATIC,
                leftNeighborID = SNAP_AUTOMATIC,
                rightNeighborID = SNAP_AUTOMATIC,
                fullyImmutable = true
            };
            panelRectangle.Y += 80;

            RepositionSliders(panelRectangle);

            okButton = new ClickableTextureComponent(new Rectangle(colorPane.Right - 80, colorPane.Bottom - 80, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = REGION_OK_BUTTON,
                upNeighborID = REGION_LIGHTNESS_SLIDER
            };

            populateClickableComponentList();
        }

        protected virtual void RepositionSliders(Rectangle panelRectangle)
        {
            sliderHandles.Clear();
            colorDrawPosition = new Vector2(panelRectangle.Right - 64, panelRectangle.Y);

            int sliderWidth = panelRectangle.Width - 100;
            int sliderY = panelRectangle.Y;

            hueSlider = new PaintColorSlider(this, REGION_HUE_SLIDER, new Rectangle(panelRectangle.Left, sliderY, sliderWidth, 12), PaintColor.MIN_HUE, PaintColor.MAX_HUE, OnSliderValueSet);
            hueSlider.getDrawColor = (float value) => GetColorForValues(value, 100f);
            hueSlider.SetValue(colorTarget.Hue, skipValueSet: true);
            sliderY += SLIDER_ROW_HEIGHT;

            saturationSlider = new PaintColorSlider(this, REGION_SATURATION_SLIDER, new Rectangle(panelRectangle.Left, sliderY, sliderWidth, 12), PaintColor.MIN_SATURATION, PaintColor.MAX_SATURATION, OnSliderValueSet);
            saturationSlider.getDrawColor = (float value) => GetColorForValues(hueSlider.GetValue(), value);
            saturationSlider.SetValue(colorTarget.Saturation, skipValueSet: true);
            sliderY += SLIDER_ROW_HEIGHT;

            lightnessSlider = new PaintColorSlider(this, REGION_LIGHTNESS_SLIDER, new Rectangle(panelRectangle.Left, sliderY, sliderWidth, 12), PaintColor.MIN_BRIGHTNESS, PaintColor.MAX_BRIGHTNESS, OnSliderValueSet);
            lightnessSlider.getDrawColor = (float value) => GetColorForValues(hueSlider.GetValue(), saturationSlider.GetValue(), value);
            lightnessSlider.SetValue(colorTarget.Lightness, skipValueSet: true);

            if (!colorTarget.HasColor)
            {
                hueSlider.SetValue(hueSlider.min, skipValueSet: true);
                saturationSlider.SetValue(saturationSlider.max, skipValueSet: true);
                lightnessSlider.SetValue((lightnessSlider.min + lightnessSlider.max) / 2, skipValueSet: true);
            }

            hueSlider.handle.upNeighborID = REGION_COPY_COLOR;
            hueSlider.handle.downNeighborID = REGION_SATURATION_SLIDER;
            saturationSlider.handle.upNeighborID = REGION_HUE_SLIDER;
            saturationSlider.handle.downNeighborID = REGION_LIGHTNESS_SLIDER;
            lightnessSlider.handle.upNeighborID = REGION_SATURATION_SLIDER;
            lightnessSlider.handle.downNeighborID = REGION_OK_BUTTON;

            sliderHandles.Add(hueSlider.handle);
            sliderHandles.Add(saturationSlider.handle);
            sliderHandles.Add(lightnessSlider.handle);
        }

        protected virtual void OnSliderValueSet(int value)
        {
            colorTarget.HasColor = true;
            ApplyColors();
        }

        public virtual void ApplyColors()
        {
            colorTarget.Hue = hueSlider.GetValue();
            colorTarget.Saturation = saturationSlider.GetValue();
            colorTarget.Lightness = lightnessSlider.GetValue();
            onColorChanged?.Invoke(colorTarget);
        }

        public Color GetColorForValues(float hue, float saturation)
        {
            Utility.HSLtoRGB(hue, saturation / 100f, 0.5, out var red, out var green, out var blue);
            return new Color((byte)red, green, blue);
        }

        public Color GetColorForValues(float hue, float saturation, float lightness)
        {
            float normalizedLightness = (lightness - (float)lightnessSlider.min) / (lightnessSlider.max - lightnessSlider.min);
            Utility.HSLtoRGB(hue, saturation / 100f, Utility.Lerp(0.25f, 0.5f, normalizedLightness), out var red, out var green, out var blue);
            return new Color((byte)red, green, blue);
        }

        public virtual bool SaveColor()
        {
            if (colorTarget.HasColor is false)
            {
                return false;
            }

            float normalizedLightness = (float)(lightnessSlider.GetValue() - lightnessSlider.min) / (lightnessSlider.max - lightnessSlider.min);
            if (savedColors.Count >= MAX_SAVED_COLORS)
            {
                savedColors.RemoveAt(0);
            }
            savedColors.Add(new Vector3(hueSlider.GetValue(), saturationSlider.GetValue(), normalizedLightness));
            return true;
        }

        public override void applyMovementKey(int direction)
        {
            if (direction == 1 || direction == 3)
            {
                foreach (PaintColorSlider slider in new[] { hueSlider, saturationSlider, lightnessSlider })
                {
                    if (slider.handle == currentlySnappedComponent)
                    {
                        slider.ApplyMovementKey(direction);
                        return;
                    }
                }
            }
            base.applyMovementKey(direction);
        }

        public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
        {
            if (a.region == REGION_COLOR_BUTTONS && b.region != REGION_COLOR_BUTTONS)
            {
                switch (direction)
                {
                    case 1:
                    case 3:
                        return false;
                    case 2:
                        if (b.myID != REGION_HUE_SLIDER)
                        {
                            return false;
                        }
                        break;
                }
            }
            return base.IsAutomaticSnapValid(direction, a, b);
        }

        public override void update(GameTime time)
        {
            activeSlider?.Update(Game1.getMouseX(), Game1.getMouseY());
            base.update(time);
        }

        public override void releaseLeftClick(int x, int y)
        {
            activeSlider = null;
            base.releaseLeftClick(x, y);
        }

        public override void performHoverAction(int x, int y)
        {
            hoverText = "";
            okButton.tryHover(x, y);
            copyColorButton.tryHover(x, y);
            defaultColorButton.tryHover(x, y);
            foreach (ClickableTextureComponent savedColorButton in savedColorButtons)
            {
                savedColorButton.tryHover(x, y);
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            for (int i = 0; i < savedColorButtons.Count; i++)
            {
                if (savedColorButtons[i].containsPoint(x, y))
                {
                    savedColors.RemoveAt(i);
                    RepositionElements();
                    Game1.playSound("coin");
                    return;
                }
            }
            base.receiveRightClick(x, y, playSound);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            hueSlider?.ReceiveLeftClick(x, y);
            saturationSlider?.ReceiveLeftClick(x, y);
            lightnessSlider?.ReceiveLeftClick(x, y);

            if (defaultColorButton.containsPoint(x, y))
            {
                colorTarget.SetColor(null);
                onColorChanged?.Invoke(colorTarget);
                Game1.playSound("coin");
                RepositionElements();
                return;
            }

            for (int i = 0; i < savedColorButtons.Count; i++)
            {
                if (savedColorButtons[i].containsPoint(x, y))
                {
                    hueSlider.SetValue((int)savedColors[i].X);
                    saturationSlider.SetValue((int)savedColors[i].Y);
                    lightnessSlider.SetValue((int)Utility.Lerp(lightnessSlider.min, lightnessSlider.max, savedColors[i].Z));
                    Game1.playSound("coin");
                    return;
                }
            }

            if (copyColorButton.containsPoint(x, y))
            {
                if (SaveColor())
                {
                    Game1.playSound("coin");
                    RepositionElements();
                }
                else
                {
                    Game1.playSound("cancel");
                }
            }
            else if (okButton.containsPoint(x, y))
            {
                exitThisMenu(playSound);
            }
            else
            {
                base.receiveLeftClick(x, y, playSound);
            }
        }

        public virtual Color? GetPreviewTint()
        {
            return colorTarget.GetColor();
        }

        protected virtual void DrawPreview(SpriteBatch b)
        {
            int drawX = previewPane.X + previewPane.Width / 2 - (int)(baseSourceRect.Width * PREVIEW_SCALE / 2f);
            int drawY = previewPane.Y + previewPane.Height / 2 - (int)(baseSourceRect.Height * PREVIEW_SCALE / 2f);
            Vector2 drawPosition = new Vector2(drawX, drawY);

            b.Draw(baseTexture, drawPosition, baseSourceRect, Color.White, 0f, Vector2.Zero, PREVIEW_SCALE, SpriteEffects.None, 0f);

            Color? tint = GetPreviewTint();
            if (tint.HasValue)
            {
                b.Draw(recolorTexture, drawPosition, recolorSourceRect, tint.Value, 0f, Vector2.Zero, PREVIEW_SCALE, SpriteEffects.None, 0f);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            }
            Game1.DrawBox(previewPane.X, previewPane.Y, previewPane.Width, previewPane.Height);

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
            Rectangle previousScissorRectangle = b.GraphicsDevice.ScissorRectangle;
            b.GraphicsDevice.ScissorRectangle = previewPane;
            DrawPreview(b);
            b.End();
            b.GraphicsDevice.ScissorRectangle = previousScissorRectangle;
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            Game1.DrawBox(colorPane.X, colorPane.Y, colorPane.Width, colorPane.Height);

            if (!string.IsNullOrEmpty(title))
            {
                int textHeight = SpriteText.getHeightOfString(title);
                SpriteText.drawStringHorizontallyCenteredAt(b, title, colorPane.X + colorPane.Width / 2, colorPane.Y + 32 - textHeight / 2);
            }

            if (colorTarget.HasColor)
            {
                b.Draw(Game1.staminaRect, new Rectangle((int)colorDrawPosition.X - 4, (int)colorDrawPosition.Y - 4, 72, 72), null, Game1.textColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);
                b.Draw(Game1.staminaRect, new Rectangle((int)colorDrawPosition.X, (int)colorDrawPosition.Y, 64, 64), null, colorTarget.GetColor().Value, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            }

            hueSlider?.Draw(b);
            saturationSlider?.Draw(b);
            lightnessSlider?.Draw(b);
            okButton.draw(b);
            copyColorButton.draw(b);
            defaultColorButton.draw(b);

            for (int i = 0; i < savedColorButtons.Count; i++)
            {
                savedColorButtons[i].draw(b, buttonColors[i], 1f);
            }

            drawMouse(b);
            if (!string.IsNullOrEmpty(hoverText))
            {
                drawHoverText(b, hoverText, Game1.dialogueFont);
            }
        }
    }
}