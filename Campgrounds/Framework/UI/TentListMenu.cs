using Campgrounds.Framework.Managers;
using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Objects;
using Campgrounds.Framework.UI.Menus;
using Campgrounds.Framework.UI.Menus.Common;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static HarmonyLib.Code;
using static System.Net.Mime.MediaTypeNames;

namespace Campgrounds.Framework.UI
{
    public class TentListMenu : IClickableMenu
    {
        private Texture2D _backgroundTexture;
        private Rectangle _tentInfoDisplayBox;
        private Rectangle _tentPreviewBox;
        private Rectangle _tentsDisplayBox;
        private Vector2 _campingTentsDisplayPosition;
        private Vector2 _tentNamePosition;
        private Vector2 _infoDisplayPosition;
        private Vector2 _buffDisplayPosition;

        private int _currentPage;
        private int TENTS_PER_PAGE = 6;
        private List<List<CampingTentData>> _pages;
        private List<ClickableComponent> _campingTentButtons = new List<ClickableComponent>();
        private ClickableTextureComponent _backButton;
        private ClickableTextureComponent _forwardButton;
        private OptionsButton _activateButton;
        private ClickableTextureComponent _paintButton;

        private CampingTentData _selectedCampingTent;
        private string _hoverHint;

        public TentListMenu() : base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
        {
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            _backgroundTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
            base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);

            SetupLayout();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && _currentPage > 0)
            {
                _currentPage--;
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && _currentPage < _pages.Count - 1)
            {
                _currentPage++;
                Game1.playSound("shiny4");
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            if (_paintButton.containsPoint(x, y) && _paintButton.visible is true)
            {
                PaintColor paintColor = new PaintColor(Campgrounds.tentManager.GetTentColor(Game1.player, _selectedCampingTent.Id));

                var paintMenu = new TentPaintingMenu(_selectedCampingTent, paintColor, "Tent Paint")
                {
                    onColorChanged = (PaintColor color) => Campgrounds.tentManager.SetTentColor(Game1.player, _selectedCampingTent.Id, color.GetColor())
                };
                base.SetChildMenu(paintMenu);
            }

            for (int i = 0; i < _campingTentButtons.Count; i++)
            {
                if (!(_pages.Count > 0 && _pages[_currentPage].Count > i))
                {
                    continue;
                }

                // Check if the tents are being clicked
                if (_campingTentButtons[i].containsPoint(x, y) && _pages[_currentPage][i].IsUnlocked())
                {
                    _selectedCampingTent = _pages[_currentPage][i];

                    _paintButton.visible = false;
                    if (_selectedCampingTent is not null && string.IsNullOrEmpty(_selectedCampingTent.GrayscaleTexturePath) is false)
                    {
                        _paintButton.visible = true;
                    }
                }
            }

            if (_currentPage < _pages.Count - 1 && _forwardButton.containsPoint(x, y))
            {
                _currentPage++;
                Game1.playSound("shwip");
                if (Game1.options.SnappyMenus && _currentPage == _pages.Count - 1)
                {
                    base.currentlySnappedComponent = base.getComponentWithID(0);
                    snapCursorToCurrentSnappedComponent();
                }
                return;
            }
            if (_currentPage > 0 && _backButton.containsPoint(x, y))
            {
                _currentPage--;
                Game1.playSound("shwip");
                if (Game1.options.SnappyMenus && _currentPage == 0)
                {
                    base.currentlySnappedComponent = base.getComponentWithID(0);
                    snapCursorToCurrentSnappedComponent();
                }
                return;
            }

            if (_selectedCampingTent is not null && _activateButton.bounds.Contains(x, y))
            {
                _activateButton.receiveLeftClick(x, y);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            _hoverHint = "";

            base.performHoverAction(x, y);

            _paintButton.tryHover(x, y);
            if (_paintButton.containsPoint(x, y))
            {
                _hoverHint = _paintButton.name;
                return;
            }

            for (int i = 0; i < _campingTentButtons.Count; i++)
            {
                if (!(_pages.Count > 0 && _pages[_currentPage].Count > i))
                {
                    continue;
                }

                // Check if the tent is being hovered
                if (_campingTentButtons[i].containsPoint(x, y, 4))
                {
                    if (_pages[_currentPage][i].IsUnlocked() is false)
                    {
                        _hoverHint = _pages[_currentPage][i].UnlockHint;
                    }
                    else
                    {
                        var tentBuffs = "";

                        var buffs = _pages[_currentPage][i].GetBuffs();
                        tentBuffs = $"\n - {string.Join("\n - ", buffs.Select(b => b.displayName))}";
                        if (_pages[_currentPage][i].NumberOfAllowedCampfireMeals > 1)
                        {
                            tentBuffs += $"\n - {Campgrounds.modHelper.Translation.Get("ui.text.bonusMeals", new
                            {
                                count = _pages[_currentPage][i].NumberOfAllowedCampfireMeals - 1
                            })}";
                        }
                        else if (buffs.Count == 0)
                        {
                            tentBuffs = Campgrounds.modHelper.Translation.Get("ui.text.none");
                        }

                        _hoverHint = Campgrounds.modHelper.Translation.Get("ui.text.buffs", new
                        {
                            buffs = tentBuffs
                        });
                    }
                }
            }
        }

        private void SetupLayout()
        {
            int x = xPositionOnScreen + 64 - 12;
            int y = yPositionOnScreen + IClickableMenu.borderWidth;
            Rectangle left_pane_rectangle = new Rectangle(x, y, 400, 720 - IClickableMenu.borderWidth * 2);
            Rectangle content_rectangle = new Rectangle(x, y, 1204, 720 - IClickableMenu.borderWidth * 2);
            content_rectangle.X += left_pane_rectangle.Width;
            content_rectangle.Width -= left_pane_rectangle.Width;
            _tentInfoDisplayBox = new Rectangle(left_pane_rectangle.X, left_pane_rectangle.Y, left_pane_rectangle.Width, left_pane_rectangle.Height);
            left_pane_rectangle.Y += 32;
            left_pane_rectangle.Height -= 32;
            int mapDisplayWidth = 320;
            _tentPreviewBox = new Rectangle(_tentInfoDisplayBox.X + (400 - mapDisplayWidth) / 2, left_pane_rectangle.Y, mapDisplayWidth, 200);
            left_pane_rectangle.Y += 230;
            left_pane_rectangle.Height -= 192;
            _tentNamePosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
            left_pane_rectangle.Y += 128;
            left_pane_rectangle.Height -= 128;
            _infoDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
            _buffDisplayPosition = new Vector2(left_pane_rectangle.Center.X, _infoDisplayPosition.Y + 72);

            _tentsDisplayBox = new Rectangle(_tentInfoDisplayBox.X + _tentInfoDisplayBox.Width + 40, base.yPositionOnScreen + 108, base.width - _tentInfoDisplayBox.Width - 128, ((base.height - 32) / 8) * TENTS_PER_PAGE);
            for (int i = 0; i <= TENTS_PER_PAGE; i++)
            {
                ClickableComponent packButton = new ClickableComponent(new Rectangle(_tentsDisplayBox.X, _tentsDisplayBox.Y + i * ((base.height - 32) / 8), base.width - _tentInfoDisplayBox.Width - 128, (base.height - 32) / 8), string.Concat(i))
                {
                    myID = i,
                    downNeighborID = i < TENTS_PER_PAGE - 1 ? i + 1 : -1,
                    upNeighborID = i > 0 ? i - 1 : -1,
                    rightNeighborID = i + 200,
                    leftNeighborID = 102
                };
                _campingTentButtons.Add(packButton);
            }
            _campingTentsDisplayPosition = new Vector2(_tentsDisplayBox.X + _tentsDisplayBox.Width / 2, _tentsDisplayBox.Y - 84);

            // Set up the various other buttons
            _backButton = new ClickableTextureComponent(new Rectangle(_tentsDisplayBox.X + 16, _tentsDisplayBox.Y + _tentsDisplayBox.Height + 32, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
            {
                myID = 102,
                rightNeighborID = -7777
            };
            _forwardButton = new ClickableTextureComponent(new Rectangle(_tentsDisplayBox.X + _tentsDisplayBox.Width - 64, _tentsDisplayBox.Y + _tentsDisplayBox.Height + 32, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
            {
                myID = 101
            };
            _paintButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_PaintBuildings"), new Rectangle(_tentInfoDisplayBox.X + _tentInfoDisplayBox.Width - 72, _tentInfoDisplayBox.Y + 192, 64, 64), null, null, Game1.mouseCursors2, new Microsoft.Xna.Framework.Rectangle(80, 208, 16, 16), 4f)
            {
                myID = 105,
                rightNeighborID = -99998,
                leftNeighborID = -99998,
                upNeighborID = 109,
                visible = false
            };

            string _activateButtonText = Campgrounds.modHelper.Translation.Get("ui.buttons.setTent.name");
            var textSize = Game1.dialogueFont.MeasureString(_activateButtonText);

            int buttonWidth = (int)textSize.X + 64;
            int buttonHeight = (int)textSize.Y + 24;

            _activateButton = new OptionsButton(_activateButtonText, () => SetTentAsActive());
            _activateButton.bounds = new Rectangle((_tentInfoDisplayBox.Width - buttonWidth), _tentInfoDisplayBox.Y + _tentInfoDisplayBox.Height - 96, buttonWidth, buttonHeight);

            PaginatePacks(Campgrounds.tentManager.CampingTentData.Where(c => c.HideUntilUnlocked is false).ToList());
        }

        public void SetTentAsActive()
        {
            if (_selectedCampingTent is null)
            {
                return;
            }

            Campgrounds.tentManager.SetCurrentTent(Game1.player, _selectedCampingTent);
        }

        public void PaginatePacks(List<CampingTentData> tents)
        {
            _pages = new List<List<CampingTentData>>();

            int count = tents.Count - 1;
            foreach (var contentPack in tents.OrderByDescending(c => c.Id == TentManager.STARTER_TENT_ID).ThenBy(c => c.IsUnlocked()).ThenBy(p => p.Id))
            {
                int which = tents.Count - 1 - count;
                int page = which / TENTS_PER_PAGE;

                while (_pages.Count <= page)
                {
                    _pages.Add(new List<CampingTentData>());
                }

                _pages[page].Add(contentPack);

                count--;
            }

            _currentPage = Math.Min(Math.Max(_currentPage, 0), _pages.Count - 1);
        }

        public void DrawCropped(SpriteBatch b, Texture2D tex, Rectangle source, Vector2 position, float scale, Rectangle clipBox, Color color)
        {
            Rectangle destination = new Rectangle((int)MathF.Floor(position.X), (int)MathF.Floor(position.Y), (int)(source.Width * scale), (int)(source.Height * scale));

            if (!destination.Intersects(clipBox))
            {
                return;
            }

            // Get overflow per edge (rounded up) so not to draw outside
            int cropL = (int)MathF.Ceiling(Math.Max(0, clipBox.X - destination.X) / scale);
            int cropT = (int)MathF.Ceiling(Math.Max(0, clipBox.Y - destination.Y) / scale);
            int cropR = (int)MathF.Ceiling(Math.Max(0, destination.Right - clipBox.Right) / scale);
            int cropB = (int)MathF.Ceiling(Math.Max(0, destination.Bottom - clipBox.Bottom) / scale);

            int w = source.Width - cropL - cropR;
            int h = source.Height - cropT - cropB;
            if (w <= 0 || h <= 0)
            {
                return;
            }

            Rectangle croppedSource = new Rectangle(source.X + cropL, source.Y + cropT, w, h);
            Vector2 drawPosition = new Vector2(destination.X + cropL * scale, destination.Y + cropT * scale);

            b.Draw(tex, drawPosition, croppedSource, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);

        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            }
            b.Draw(_backgroundTexture, new Vector2(xPositionOnScreen + width / 2, yPositionOnScreen + height / 2), new Rectangle(0, 0, 320, 180), Color.White, 0f, new Vector2(160f, 90f), 4f, SpriteEffects.None, 0.86f);

            Game1.DrawBox(_tentInfoDisplayBox.X, _tentInfoDisplayBox.Y, _tentInfoDisplayBox.Width, _tentInfoDisplayBox.Height);
            Game1.DrawBox(_tentPreviewBox.X, _tentPreviewBox.Y, _tentPreviewBox.Width, _tentPreviewBox.Height);

            var tentName = "";
            var secondaryTentName = "";
            var tentDescription = "";
            if (_selectedCampingTent is not null && _selectedCampingTent.IsUnlocked())
            {
                (tentName, secondaryTentName) = TextHelper.SplitLabel(_selectedCampingTent.DisplayName);

                tentDescription = _selectedCampingTent.Description;
            }
            else
            {
                int maxLength = Campgrounds.modHelper.Translation.LocaleEnum is LocalizedContentManager.LanguageCode.en ? 7 : 16;
                (tentName, secondaryTentName) = TextHelper.SplitLabel(Campgrounds.modHelper.Translation.Get("ui.text.selectPromptTent"), maxLength);
            }

            SpriteText.drawStringHorizontallyCenteredAt(b, tentName, (int)_tentNamePosition.X + 4, (int)_tentNamePosition.Y);
            if (string.IsNullOrEmpty(secondaryTentName) is false)
            {
                SpriteText.drawStringHorizontallyCenteredAt(b, secondaryTentName, (int)_tentNamePosition.X - 8, (int)_tentNamePosition.Y + 48);
            }
            
            if (string.IsNullOrEmpty(tentDescription) is false)
            {
                var wrappedText = Game1.parseText(tentDescription, Game1.smallFont, _tentInfoDisplayBox.Width - 48);
                Utility.drawTextWithShadow(b, wrappedText, Game1.smallFont, new Vector2(_tentInfoDisplayBox.X + 32, _infoDisplayPosition.Y - (string.IsNullOrEmpty(secondaryTentName) ? 64 : 24)), Game1.textColor);
            }

            // Draw the tent buttons
            SpriteText.drawStringWithScrollCenteredAt(b, Campgrounds.modHelper.Translation.Get("ui.text.tents"), (int)_campingTentsDisplayPosition.X, (int)_campingTentsDisplayPosition.Y);
            Game1.DrawBox(_tentsDisplayBox.X, _tentsDisplayBox.Y, _tentsDisplayBox.Width, _tentsDisplayBox.Height);
            for (int j = 0; j < _campingTentButtons.Count; j++)
            {
                if (_pages.Count() > 0 && _pages[_currentPage].Count() > j)
                {
                    var tent = _pages[_currentPage][j];

                    var tentButtonText = tent.DisplayName;
                    if (tentButtonText.Length > 32)
                    {
                        tentButtonText = $"{tentButtonText.Substring(0, 32).TrimEnd()}...";
                    }

                    var tentButtonColor = _campingTentButtons[j].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White;
                    if (tent.Id == Campgrounds.tentManager.GetCurrentTent(Game1.player).Id)
                    {
                        tentButtonColor = Color.Wheat;
                    }

                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), _campingTentButtons[j].bounds.X, _campingTentButtons[j].bounds.Y, _campingTentButtons[j].bounds.Width, _campingTentButtons[j].bounds.Height, tentButtonColor, 4f, drawShadow: false);
                    if (tent.IsUnlocked() is false)
                    {
                        SpriteText.drawString(b, "???", _campingTentButtons[j].bounds.X + 58, _campingTentButtons[j].bounds.Y + 20, color: Color.Black * 0.35f, alpha: 0.89f);
                    }
                    else
                    {
                        SpriteText.drawString(b, tentButtonText, _campingTentButtons[j].bounds.X + 58, _campingTentButtons[j].bounds.Y + 20, color: new Color(86, 22, 12));
                    }
                }
            }

            // Draw page buttons
            if (_currentPage < _pages.Count - 1)
            {
                _forwardButton.draw(b);
            }
            if (_currentPage > 0)
            {
                _backButton.draw(b);
            }

            // Draw paint button
            _paintButton.draw(b);

            if (_selectedCampingTent is not null && _selectedCampingTent.IsUnlocked() is true)
            {
                Texture2D previewCampingTentTexture = Campgrounds.modHelper.GameContent.Load<Texture2D>(_selectedCampingTent.TexturePath);
                float previewCampsiteScale = 3f;
                Rectangle campingTentBoundary = _selectedCampingTent.SouthSprite.DisplayRectangle;

                var centerPosition = new Vector2(MathF.Floor(_tentPreviewBox.X + (_tentPreviewBox.Width - campingTentBoundary.Width * previewCampsiteScale) / 2f), MathF.Floor(_tentPreviewBox.Y + (_tentPreviewBox.Height - campingTentBoundary.Height * previewCampsiteScale) / 2f - 8));
                var cropBoundary = new Rectangle(_tentPreviewBox.X, _tentPreviewBox.Y, _tentPreviewBox.Width, _tentPreviewBox.Height);

                DrawCropped(b, previewCampingTentTexture, campingTentBoundary, centerPosition + _selectedCampingTent.PreviewOffset, previewCampsiteScale, cropBoundary, Color.White);

                var tentColor = Campgrounds.tentManager.GetTentColor(Game1.player, _selectedCampingTent.Id);
                if (tentColor is not null)
                {
                    Texture2D previewCampingTentTextureGrayscale = Campgrounds.modHelper.GameContent.Load<Texture2D>(_selectedCampingTent.GrayscaleTexturePath);
                    DrawCropped(b, previewCampingTentTextureGrayscale, campingTentBoundary, centerPosition + _selectedCampingTent.PreviewOffset, previewCampsiteScale, cropBoundary, tentColor.Value);
                }

                if (_selectedCampingTent.Id != Campgrounds.tentManager.GetCurrentTent(Game1.player).Id)
                {
                    _activateButton.draw(b, 0, 0);
                }
                else
                {
                    SpriteText.drawStringWithScrollCenteredAt(b, Campgrounds.modHelper.Translation.Get("ui.text.currentTent"), _tentInfoDisplayBox.Center.X, _activateButton.bounds.Y);
                }
            }

            base.draw(b);
            base.drawMouse(b, ignore_transparency: true);

            if (string.IsNullOrEmpty(_hoverHint) is false && GetChildMenu() is null)
            {
                drawHoverText(b, _hoverHint, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null);
            }
        }
    }
}
