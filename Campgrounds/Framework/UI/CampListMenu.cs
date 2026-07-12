using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Campgrounds.Framework.UI
{
    public class CampListMenu : IClickableMenu
    {
        private Texture2D _backgroundTexture;
        private Rectangle _campsiteSummaryDisplayBox;
        private Rectangle _mapDisplayBox;
        private Rectangle _campsitesDisplayBox;
        private Vector2 _campSitesDisplayPosition;
        private Vector2 _campsiteNamePosition;
        private Vector2 _infoDisplayPosition;

        private int _currentPage;
        private int SITES_PER_PAGE = 6;
        private List<List<CampgroundData>> _pages;
        private List<ClickableComponent> _campsiteButtons = new List<ClickableComponent>();
        private ClickableTextureComponent _backButton;
        private ClickableTextureComponent _forwardButton;
        private OptionsButton _travelButton;

        private CampgroundData _selectedCampsite;
        private string _hoverHint;

        public CampListMenu() : base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
        {
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            _backgroundTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
            base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);

            SetupLayout();

            Campgrounds.currencyManager.ShowCurrency(Models.Enums.Currency.CampRations, () => Game1.activeClickableMenu == this, 0f);
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

            for (int i = 0; i < _campsiteButtons.Count; i++)
            {
                if (!(_pages.Count > 0 && _pages[_currentPage].Count > i))
                {
                    continue;
                }

                // Check if the campsites are being clicked
                if (_campsiteButtons[i].containsPoint(x, y))
                {
                    _selectedCampsite = _pages[_currentPage][i];
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

            if (_selectedCampsite is not null && _travelButton.bounds.Contains(x, y))
            {
                _travelButton.receiveLeftClick(x, y);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            _hoverHint = "";
            for (int i = 0; i < _campsiteButtons.Count; i++)
            {
                if (!(_pages.Count > 0 && _pages[_currentPage].Count > i))
                {
                    continue;
                }

                // Check if the campsite is being hovered
                if (_campsiteButtons[i].containsPoint(x, y, 4) && _pages[_currentPage][i].IsUnlocked() is false)
                {
                    _hoverHint = _pages[_currentPage][i].UnlockHint;
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
            _campsiteSummaryDisplayBox = new Rectangle(left_pane_rectangle.X, left_pane_rectangle.Y, left_pane_rectangle.Width, left_pane_rectangle.Height);
            left_pane_rectangle.Y += 32;
            left_pane_rectangle.Height -= 32;
            int mapDisplayWidth = 320;
            _mapDisplayBox = new Rectangle(_campsiteSummaryDisplayBox.X + (400 - mapDisplayWidth) / 2, left_pane_rectangle.Y, mapDisplayWidth, 160);
            left_pane_rectangle.Y += 192;
            left_pane_rectangle.Height -= 192;
            _campsiteNamePosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
            left_pane_rectangle.Y += 128;
            left_pane_rectangle.Height -= 128;
            _infoDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);

            _campsitesDisplayBox = new Rectangle(_campsiteSummaryDisplayBox.X + _campsiteSummaryDisplayBox.Width + 40, base.yPositionOnScreen + 108, base.width - _campsiteSummaryDisplayBox.Width - 128, ((base.height - 32) / 8) * SITES_PER_PAGE);
            for (int i = 0; i <= SITES_PER_PAGE; i++)
            {
                ClickableComponent packButton = new ClickableComponent(new Rectangle(_campsitesDisplayBox.X, _campsitesDisplayBox.Y + i * ((base.height - 32) / 8), base.width - _campsiteSummaryDisplayBox.Width - 128, (base.height - 32) / 8), string.Concat(i))
                {
                    myID = i,
                    downNeighborID = i < SITES_PER_PAGE - 1 ? i + 1 : -1,
                    upNeighborID = i > 0 ? i - 1 : -1,
                    rightNeighborID = i + 200,
                    leftNeighborID = 102
                };
                _campsiteButtons.Add(packButton);
            }
            _campSitesDisplayPosition = new Vector2(_campsitesDisplayBox.X + _campsitesDisplayBox.Width / 2, _campsitesDisplayBox.Y - 84);

            // Set up the various other buttons
            _backButton = new ClickableTextureComponent(new Rectangle(_campsitesDisplayBox.X + 16, _campsitesDisplayBox.Y + _campsitesDisplayBox.Height + 32, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
            {
                myID = 102,
                rightNeighborID = -7777
            };
            _forwardButton = new ClickableTextureComponent(new Rectangle(_campsitesDisplayBox.X + _campsitesDisplayBox.Width - 64, _campsitesDisplayBox.Y + _campsitesDisplayBox.Height + 32, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
            {
                myID = 101
            };

            _travelButton = new OptionsButton("Travel", () => StartTravelingToCampsite(skipRationCheck: false));
            _travelButton.bounds.X = _campsiteSummaryDisplayBox.X + _travelButton.bounds.Width / 2 + 8;
            _travelButton.bounds.Y = _campsiteSummaryDisplayBox.Y + _campsiteSummaryDisplayBox.Height - 96;

            PaginatePacks(Campgrounds.campManager.CampgroundData.Where(c => c.HideUntilUnlocked is false).OrderByDescending(c => c.IsUnlocked()).ThenBy(c => c.TravelTimeInHours).ToList());
        }

        public void StartTravelingToCampsite(bool skipRationCheck = false)
        {
            if (_selectedCampsite.IsUnlocked() is false)
            {
                return;
            }

            if (skipRationCheck is false && Campgrounds.currencyManager.GetCurrencyBalance(Models.Enums.Currency.CampRations) <= 0)
            {
                Game1.currentLocation.createQuestionDialogue("Are you sure you want to travel without rations? You will not be able to make food at the campfire.", Game1.currentLocation.createYesNoResponses(), (Farmer who, string answer) => CampingHelper.OnLeaveWithoutRationsResponse(who, answer, this));
            }
            else
            {
                this.exitThisMenu(playSound: false);

                // Handle traveling to campsite (verify enough time to travel, don't allow if will reach campsite at or after midnight)
                if (Game1.timeOfDay + (_selectedCampsite.TravelTimeInHours * 100) >= 2400)
                {
                    Game1.activeClickableMenu = new DialogueBox("There is not enough time to reach that campsite before midnight...");
                    return;
                }
                if (_selectedCampsite.RequireVehicle is true && CampingHelper.IsCarRepaired() is false)
                {
                    var dialogue = new List<string>()
                {
                    "This campsite requires a vehicle to reach. You see a note attached to the map...",
                    "\"You can use my grandmother's old car in the garage! It will need some work to get running again though.\" - Vesi"
                };
                    Game1.activeClickableMenu = new DialogueBox(dialogue);
                    return;
                }

                // Display travel screen with any TravelScreenText (or use default if none given) and warp to campsite
                Campgrounds.campManager.StartTraveling(Game1.player, _selectedCampsite);
            }
        }

        public void PaginatePacks(List<CampgroundData> campsites)
        {
            _pages = new List<List<CampgroundData>>();

            int count = campsites.Count - 1;
            foreach (var contentPack in campsites.OrderBy(p => p.Id))
            {
                int which = campsites.Count - 1 - count;
                int page = which / SITES_PER_PAGE;

                while (_pages.Count <= page)
                {
                    _pages.Add(new List<CampgroundData>());
                }

                _pages[page].Add(contentPack);

                count--;
            }

            _currentPage = Math.Min(Math.Max(_currentPage, 0), _pages.Count - 1);
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            }
            b.Draw(_backgroundTexture, new Vector2(xPositionOnScreen + width / 2, yPositionOnScreen + height / 2), new Rectangle(0, 0, 320, 180), Color.White, 0f, new Vector2(160f, 90f), 4f, SpriteEffects.None, 0.86f);

            Game1.DrawBox(_campsiteSummaryDisplayBox.X, _campsiteSummaryDisplayBox.Y, _campsiteSummaryDisplayBox.Width, _campsiteSummaryDisplayBox.Height);
            Game1.DrawBox(_mapDisplayBox.X, _mapDisplayBox.Y, _mapDisplayBox.Width, _mapDisplayBox.Height);
            //b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, _characterSpriteDrawPosition, Color.White);

            var campsiteName = "Select";
            var secondaryCampsiteName = "";
            var travelTimeInfo = $"";
            var vehicleRequiredInfo = $"";
            if (_selectedCampsite is not null && _selectedCampsite.IsUnlocked() && Game1.locationData.ContainsKey(_selectedCampsite.Id))
            {
                campsiteName = Game1.locationData[_selectedCampsite.Id].DisplayName;
                if (campsiteName.Length > 16)
                {
                    secondaryCampsiteName = campsiteName.Substring(16, campsiteName.Length - 16);
                    campsiteName = $"{campsiteName.Substring(0, 16).TrimEnd()}";
                }

                travelTimeInfo = $"Travel Time: {_selectedCampsite.TravelTimeInHours} hr(s)";
                vehicleRequiredInfo = $"Requires Vehicle: {(_selectedCampsite.RequireVehicle ? "Yes" : "No")}";
            }
            else
            {
                secondaryCampsiteName = "a Campsite";
            }

            SpriteText.drawStringHorizontallyCenteredAt(b, campsiteName, (int)_campsiteNamePosition.X + 4, (int)_campsiteNamePosition.Y);
            if (string.IsNullOrEmpty(secondaryCampsiteName) is false)
            {
                SpriteText.drawStringHorizontallyCenteredAt(b, secondaryCampsiteName, (int)_campsiteNamePosition.X - 8, (int)_campsiteNamePosition.Y + 48);
            }
            
            if (string.IsNullOrEmpty(travelTimeInfo) is false || string.IsNullOrEmpty(vehicleRequiredInfo) is false)
            {
                SpriteText.drawStringHorizontallyCenteredAt(b, "Info", (int)_infoDisplayPosition.X, (int)_infoDisplayPosition.Y);
            }
            if (string.IsNullOrEmpty(travelTimeInfo) is false)
            {
                b.DrawString(Game1.dialogueFont, travelTimeInfo, new Vector2((0f - Game1.dialogueFont.MeasureString(travelTimeInfo).X) / 2f + _infoDisplayPosition.X, _infoDisplayPosition.Y + 64), Game1.textColor);
            }            
            if (string.IsNullOrEmpty(vehicleRequiredInfo) is false)
            {
                var vehicleRequiredColor = Game1.textColor;
                if (_selectedCampsite.RequireVehicle && CampingHelper.IsCarRepaired() is false)
                {
                    vehicleRequiredColor = Color.Red;
                }

                b.DrawString(Game1.dialogueFont, vehicleRequiredInfo, new Vector2((0f - Game1.dialogueFont.MeasureString(vehicleRequiredInfo).X) / 2f + _infoDisplayPosition.X, _infoDisplayPosition.Y + 128), vehicleRequiredColor);
            }

            // Draw the campsite buttons
            SpriteText.drawStringWithScrollCenteredAt(b, "Campsites", (int)_campSitesDisplayPosition.X, (int)_campSitesDisplayPosition.Y);
            Game1.DrawBox(_campsitesDisplayBox.X, _campsitesDisplayBox.Y, _campsitesDisplayBox.Width, _campsitesDisplayBox.Height);
            for (int j = 0; j < _campsiteButtons.Count; j++)
            {
                if (_pages.Count() > 0 && _pages[_currentPage].Count() > j)
                {
                    var campsite = _pages[_currentPage][j];
                    if (Game1.locationData.ContainsKey(campsite.Id) is false)
                    {
                        continue;
                    }

                    var locationName = Game1.locationData[campsite.Id].DisplayName;
                    if (locationName.Length > 32)
                    {
                        locationName = $"{locationName.Substring(0, 32).TrimEnd()}...";
                    }

                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), _campsiteButtons[j].bounds.X, _campsiteButtons[j].bounds.Y, _campsiteButtons[j].bounds.Width, _campsiteButtons[j].bounds.Height, _campsiteButtons[j].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
                    if (campsite.IsUnlocked() is false)
                    {
                        SpriteText.drawString(b, "???", _campsiteButtons[j].bounds.X + 58, _campsiteButtons[j].bounds.Y + 20, color: Color.Black * 0.35f, alpha: 0.89f);
                    }
                    else
                    {
                        SpriteText.drawString(b, locationName, _campsiteButtons[j].bounds.X + 58, _campsiteButtons[j].bounds.Y + 20, color: new Color(86, 22, 12));
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

            if (_selectedCampsite is not null && _selectedCampsite.IsUnlocked() is true)
            {
                Texture2D previewCampsiteTexture = Campgrounds.modHelper.GameContent.Load<Texture2D>(Campgrounds.CAMPGROUND_DEFAULT_PREVIEW_TEXTURE_PATH);
                float previewCampsiteScale = _selectedCampsite.PreviewTextureScale > 0f ? _selectedCampsite.PreviewTextureScale : 4f;
                if (string.IsNullOrEmpty(_selectedCampsite.PreviewTexturePath) is false)
                {
                    try
                    {
                        previewCampsiteTexture = Campgrounds.modHelper.GameContent.Load<Texture2D>(_selectedCampsite.PreviewTexturePath);
                    }
                    catch (Exception ex)
                    {
                        Campgrounds.monitor.LogOnce($"Failed to load preview image for campground {_selectedCampsite.Id}: {ex}", StardewModdingAPI.LogLevel.Warn);
                    }
                }

                b.Draw(previewCampsiteTexture, new Vector2(_mapDisplayBox.X, _mapDisplayBox.Y), previewCampsiteTexture.Bounds, Color.White, 0f, Vector2.Zero, previewCampsiteScale, SpriteEffects.None, 1f);

                _travelButton.draw(b, 0, 0);
            }

            base.draw(b);
            base.drawMouse(b, ignore_transparency: true);

            if (string.IsNullOrEmpty(_hoverHint) is false)
            {
                drawHoverText(b, _hoverHint, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null);
            }
        }
    }
}
