using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Objects;
using Campgrounds.Framework.UI.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace Campgrounds.Framework.UI
{
    public class CookingSpotMenu : IClickableMenu
    {
        public CampfireFoodData hoverRecipe;
        public ClickableTextureComponent upButton;
        public ClickableTextureComponent downButton;

        public string hoverText = "";
        public CampfireFoodData lastCookingHover;
        public int hoverAmount;
        private string hoverTitle = "";

        public int currentCraftingPage;
        public List<Dictionary<ClickableTextureComponent, CampfireFoodData>> pagesOfCraftingRecipes = new List<Dictionary<ClickableTextureComponent, CampfireFoodData>>();
        public List<ClickableComponent> currentPageClickableComponents = new List<ClickableComponent>();

        public List<CampfireFoodData> selectedCampfireFoods = new List<CampfireFoodData>();

        private OptionsButton _prepMealsButton;

        private Campsite _campsite;

        public CookingSpotMenu(int x, int y, int width, int height) : base(x, y, width, height, showUpperRightCloseButton: true)
        {
            _campsite = Campgrounds.campManager.GetActiveCampsiteFromLocation(Game1.currentLocation);
            if (_campsite is null)
            {
                base.exitThisMenu();
            }

            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);

            RepositionElements();
            if (Game1.options.SnappyMenus)
            {
                snapToDefaultClickableComponent();
            }
        }

        private Dictionary<ClickableTextureComponent, CampfireFoodData> createNewPage()
        {
            Dictionary<ClickableTextureComponent, CampfireFoodData> page = new Dictionary<ClickableTextureComponent, CampfireFoodData>();
            pagesOfCraftingRecipes.Add(page);
            return page;
        }

        public virtual void RepositionElements()
        {
            pagesOfCraftingRecipes.Clear();
            layoutRecipes(Campgrounds.campManager.CampfireFoodData);
            if (pagesOfCraftingRecipes.Count > 1)
            {
                upButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 768 + 32, craftingPageY(), 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12), 0.8f)
                {
                    myID = 88,
                    downNeighborID = 89,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                downButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 768 + 32, craftingPageY() + 192 + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11), 0.8f)
                {
                    myID = 89,
                    upNeighborID = 88,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
            }

            if (upButton != null)
            {
                upButton.bounds.X = xPositionOnScreen + 768 + 32;
                upButton.bounds.Y = craftingPageY();
            }
            if (downButton != null)
            {
                downButton.bounds.X = xPositionOnScreen + 768 + 32;
                downButton.bounds.Y = craftingPageY() + 192 + 32;
            }

            _prepMealsButton = new OptionsButton("Start Cooking", StartCookingFood);
            _prepMealsButton.bounds.X = xPositionOnScreen + (width + _prepMealsButton.bounds.Width) / 2 - 56;
            _prepMealsButton.bounds.Y = yPositionOnScreen + height - 108;


            _UpdateCurrentPageButtons();
        }

        private void StartCookingFood()
        {
            // Create cinematic message
            Campgrounds.messageManager.Messages.Add(new CookingMessage());

            // Cache the buffs so they can be applied in next morning
            var buffs = _campsite.CurrentCampTent.GetBuffs();
            foreach (var campfireFood in selectedCampfireFoods)
            {
                foreach (Buff buff in campfireFood.GetBuffs())
                {
                    buffs.Add(buff);
                }
            }
            _campsite.CacheBuffs(buffs);

            this.exitThisMenu(playSound: false);
        }

        private int craftingPageY()
        {
            return yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 16;
        }

        private ClickableTextureComponent[,] createNewPageLayout()
        {
            return new ClickableTextureComponent[10, 4];
        }

        private bool spaceOccupied(ClickableTextureComponent[,] pageLayout, int x, int y, CampfireFoodData campfireRecipe)
        {
            if (pageLayout[x, y] != null)
            {
                return true;
            }
            if (y + 1 < 4)
            {
                return pageLayout[x, y + 1] != null;
            }
            return true;
        }

        private void layoutRecipes(List<CampfireFoodData> campfireFoodRecipes)
        {
            int craftingPageX = xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - 16;
            int spaceBetweenCraftingIcons = 8;
            Dictionary<ClickableTextureComponent, CampfireFoodData> currentPage = createNewPage();
            int x = 0;
            int y = 0;
            int i = 0;
            ClickableTextureComponent[,] pageLayout = createNewPageLayout();
            List<ClickableTextureComponent[,]> pageLayouts = new List<ClickableTextureComponent[,]>();
            pageLayouts.Add(pageLayout);

            foreach (var campfireRecipe in campfireFoodRecipes)
            {
                i++;
                while (spaceOccupied(pageLayout, x, y, campfireRecipe))
                {
                    x++;
                    if (x >= 10)
                    {
                        x = 0;
                        y++;
                        if (y >= 4)
                        {
                            currentPage = createNewPage();
                            pageLayout = createNewPageLayout();
                            pageLayouts.Add(pageLayout);
                            x = 0;
                            y = 0;
                        }
                    }
                }
                int id = 200 + i;
                Texture2D texture = Campgrounds.modHelper.GameContent.Load<Texture2D>(campfireRecipe.TexturePath);
                Rectangle sourceRect = campfireRecipe.SourceRectangle.Value;
                ClickableTextureComponent component = new ClickableTextureComponent("", new Rectangle(craftingPageX + x * (64 + spaceBetweenCraftingIcons), craftingPageY() + y * 72, 64, 64), null, !campfireRecipe.IsUnlocked() ? "ghosted" : "", texture, sourceRect, 4f)
                {
                    myID = id,
                    rightNeighborID = -99998,
                    leftNeighborID = -99998,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    fullyImmutable = true,
                    region = 8000
                };
                currentPage.Add(component, campfireRecipe);
                pageLayout[x, y] = component;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = ((currentCraftingPage < pagesOfCraftingRecipes.Count) ? pagesOfCraftingRecipes[currentCraftingPage].First().Key : null);
            snapCursorToCurrentSnappedComponent();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && currentCraftingPage > 0)
            {
                currentCraftingPage--;
                _UpdateCurrentPageButtons();
                Game1.playSound("shwip");
                if (Game1.options.SnappyMenus)
                {
                    setCurrentlySnappedComponentTo(88);
                    snapCursorToCurrentSnappedComponent();
                }
            }
            else if (direction < 0 && currentCraftingPage < pagesOfCraftingRecipes.Count - 1)
            {
                currentCraftingPage++;
                _UpdateCurrentPageButtons();
                Game1.playSound("shwip");
                if (Game1.options.SnappyMenus)
                {
                    setCurrentlySnappedComponentTo(89);
                    snapCursorToCurrentSnappedComponent();
                }
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y);

            if (downButton != null && downButton.containsPoint(x, y) && currentCraftingPage < pagesOfCraftingRecipes.Count - 1)
            {
                Game1.playSound("coin");
                currentCraftingPage = Math.Min(pagesOfCraftingRecipes.Count - 1, currentCraftingPage + 1);
                _UpdateCurrentPageButtons();
                downButton.scale = downButton.baseScale;
            }

            foreach (ClickableTextureComponent c in pagesOfCraftingRecipes[currentCraftingPage].Keys)
            {
                int times = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : (Game1.oldKBState.IsKeyDown(Keys.LeftControl) ? 25 : 5));
                for (int i = 0; i < times; i++)
                {
                    if (c.containsPoint(x, y, 4) && !c.hoverText.Equals("ghosted"))
                    {
                        clickCraftingRecipe(c, i == 0);
                    }
                }
            }

            if (selectedCampfireFoods.Count > 0 && _prepMealsButton.bounds.Contains(x, y))
            {
                _prepMealsButton.receiveLeftClick(x, y);
            }
        }

        protected void _UpdateCurrentPageButtons()
        {
            currentPageClickableComponents.Clear();
            foreach (ClickableTextureComponent component in pagesOfCraftingRecipes[currentCraftingPage].Keys)
            {
                currentPageClickableComponents.Add(component);
            }
            populateClickableComponentList();
        }


        private void clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
        {
            CampfireFoodData campfireFoodData = pagesOfCraftingRecipes[currentCraftingPage][c];
            if (selectedCampfireFoods.Contains(campfireFoodData))
            {
                selectedCampfireFoods.Remove(campfireFoodData);
            }
            else if (selectedCampfireFoods.Count < _campsite.CurrentCampTent.NumberOfAllowedCampfireMeals)
            {
                selectedCampfireFoods.Add(campfireFoodData);
            }
            else
            {
                Game1.playSound("smallSelect");
            }
        }

        public override void performHoverAction(int x, int y)
        {
            var prevHoverRecipe = hoverRecipe;
            base.performHoverAction(x, y);
            hoverTitle = "";
            hoverText = "";
            hoverRecipe = null;
            hoverAmount = -1;

            foreach (ClickableTextureComponent c in pagesOfCraftingRecipes[currentCraftingPage].Keys)
            {
                if (c.containsPoint(x, y, 4))
                {
                    if (c.hoverText.Equals("ghosted"))
                    {
                        hoverText = "???";
                        continue;
                    }
                    hoverTitle = pagesOfCraftingRecipes[currentCraftingPage][c].DisplayName;
                    hoverText = pagesOfCraftingRecipes[currentCraftingPage][c].Description;
                    if (hoverText.Length > 32)
                    {
                        hoverText = Regex.Replace(hoverText, @"(.{20,}?)(\s+)", "$1\n");
                    }

                    hoverRecipe = pagesOfCraftingRecipes[currentCraftingPage][c];
                    if (prevHoverRecipe == null || prevHoverRecipe.Id != hoverRecipe.Id)
                    {
                        lastCookingHover = hoverRecipe;
                    }
                    c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.4f);
                }
                else
                {
                    c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
                }
            }
            if (upButton != null)
            {
                if (upButton.containsPoint(x, y))
                {
                    upButton.scale = Math.Min(upButton.scale + 0.02f, upButton.baseScale + 0.1f);
                }
                else
                {
                    upButton.scale = Math.Max(upButton.scale - 0.02f, upButton.baseScale);
                }
            }
            if (downButton != null)
            {
                if (downButton.containsPoint(x, y))
                {
                    downButton.scale = Math.Min(downButton.scale + 0.02f, downButton.baseScale + 0.1f);
                }
                else
                {
                    downButton.scale = Math.Max(downButton.scale - 0.02f, downButton.baseScale);
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
            int horizontalPartitionY = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256;
            drawHorizontalPartition(b, horizontalPartitionY);

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            foreach (ClickableTextureComponent c in pagesOfCraftingRecipes[currentCraftingPage].Keys)
            {
                CampfireFoodData campfireFoodData = pagesOfCraftingRecipes[currentCraftingPage][c];
                if (!selectedCampfireFoods.Contains(campfireFoodData))
                {
                    c.draw(b, Color.Black * 0.35f, 0.89f);
                }
                else
                {
                    c.draw(b);
                }
            }
            
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            base.draw(b);

            SpriteText.drawStringWithScrollCenteredAt(b, "Campfire Cooking", xPositionOnScreen + width / 2, yPositionOnScreen + 55);
            SpriteText.drawStringWithScrollCenteredAt(b, "Summary", xPositionOnScreen + width / 2, horizontalPartitionY + 4);

            var buffs = _campsite.CurrentCampTent.GetBuffs();
            var tentBuffs = $"\n - {string.Join("\n - ", buffs.Select(b => b.displayName))}";
            if (_campsite.CurrentCampTent.NumberOfAllowedCampfireMeals > 1)
            {
                tentBuffs += $"\n - {_campsite.CurrentCampTent.NumberOfAllowedCampfireMeals - 1} bonus meal(s)";
            }
            else if (buffs.Count == 0)
            {
                tentBuffs = "\n - None";
            }

            b.DrawString(Game1.dialogueFont, $"Tent Buffs:{tentBuffs}", new Vector2(xPositionOnScreen + +IClickableMenu.borderWidth, horizontalPartitionY + 44), Game1.textColor);
            b.DrawString(Game1.dialogueFont, $"Meals: {selectedCampfireFoods.Count}/{_campsite.CurrentCampTent.NumberOfAllowedCampfireMeals}", new Vector2(xPositionOnScreen + width - 232, horizontalPartitionY + 44), Game1.textColor);

            if (downButton != null && currentCraftingPage < pagesOfCraftingRecipes.Count - 1)
            {
                downButton.draw(b);
            }
            if (upButton != null && currentCraftingPage > 0)
            {
                upButton.draw(b);
            }

            if (selectedCampfireFoods.Count > 0)
            {
                _prepMealsButton.draw(b, 0, 0);
            }

            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
            
            if (hoverRecipe != null)
            {
                string[] buffIcons = null;
                BuffEffects effects = new BuffEffects();
                int millisecondsDuration = int.MinValue;
                foreach (Buff buff in hoverRecipe.GetBuffs())
                {
                    effects.Add(buff.effects);
                    if (buff.millisecondsDuration == -2 || (buff.millisecondsDuration > millisecondsDuration && millisecondsDuration != -2))
                    {
                        millisecondsDuration = buff.millisecondsDuration;
                    }
                }
                if (effects.HasAnyValue())
                {
                    buffIcons = effects.ToLegacyAttributeFormat();
                    if (millisecondsDuration != -2)
                    {
                        buffIcons[12] = " " + Utility.getMinutesSecondsStringFromMilliseconds(millisecondsDuration);
                    }
                }

                drawHoverText(b, hoverText, Game1.smallFont, 0, 0, -1, hoverTitle, -1, buffIcons, null, 0, null, -1, -1, -1, 1f, null, null);
            }
        }
    }
}
