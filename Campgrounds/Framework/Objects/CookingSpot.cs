using Campgrounds.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Objects
{
    public class CookingSpot : Torch
    {
        public bool CanCook { get; set; }
        public bool HasCookedToday { get; set; }

        public CookingSpot() : base("278", bigCraftable: true)
        {
            IsOn = true;
            Fragility = 2;
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
            {
                return true;
            }

            var campsite = Campgrounds.campManager.GetActiveCampsiteFromLocation(who.currentLocation);
            if (campsite is null)
            {
                return true;
            }

            if (HasCookedToday is false)
            {
                if (CanCook)
                {
                    Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
                    Game1.activeClickableMenu = new CookingSpotMenu((int)center.X, (int)center.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
                }
                else
                {
                    Game1.activeClickableMenu = new DialogueBox("Yesterday's campfire, now just a pile of ashes.");
                }
            }
            else
            {
                Game1.activeClickableMenu = new DialogueBox("You already cooked enough for today.");
            }

            return true;
        }

        public override void actionOnPlayerEntry()
        {
            base.actionOnPlayerEntry();

            base.initializeLightSource(tileLocation.Value);
        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            if (CanCook is false)
            {
                if (base.lightSource is not null)
                {
                    Game1.currentLocation.removeLightSource(base.lightSource.Id);
                    base.lightSource = null;
                }
                    
                IsOn = false;
            }
            else
            {
                if (base.lightSource is null)
                {
                    base.initializeLightSource(tileLocation.Value);
                }

                IsOn = true;

                base.updateWhenCurrentLocation(time);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (CanCook is false)
            {
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(BC)146");
                Rectangle sourceRect = itemData.GetSourceRect(0, 146);

                Vector2 scaleFactor = getScale();
                scaleFactor *= 4f;
                Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
                Rectangle destination = new Rectangle((int)(position2.X - scaleFactor.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position2.Y - scaleFactor.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));

                Texture2D texture2 = itemData.GetTexture();
                float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                spriteBatch.Draw(texture2, destination, sourceRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
            }
            else
            {
                base.draw(spriteBatch, x, y, alpha);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            base.draw(spriteBatch, xNonTile, yNonTile, layerDepth, alpha);
        }
    }
}
