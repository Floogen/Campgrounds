using Campgrounds.Framework.Managers;
using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static StardewValley.Minigames.CraneGame;

namespace Campgrounds.Framework.Objects
{
    public class CampsiteMap : ISalable
    {
        public const string CAMPSITE_MAP_ID = "PeacefulEnd.Campgrounds.Items.CampsiteMap";

        public string TypeDefinitionId => "(Salable)";

        public string QualifiedItemId => TypeDefinitionId + "CampsiteMap";

        public string DisplayName => $"Campsite Map ({GetLocationName()})";

        public string Name => CAMPSITE_MAP_ID;

        public bool IsRecipe { get { return false; } set { } }
        public int Stack { get { return 1; } set { } }
        public int Quality { get { return 0; } set { } }

        public CampgroundData CampgroundData { get; }

        public CampsiteMap(CampgroundData campgroundData)
        {
            CampgroundData = campgroundData;
        }

        public bool actionWhenPurchased(string shopId)
        {
            // Unlock the associated campground
            NetWorldState.addWorldStateIDEverywhere($"MAP_UNLOCKED_CAMPGROUND:{CampgroundData.Id}");

            // Exit current menu
            Game1.exitActiveMenu();

            // Display message
            HoldUpMap($"You have discovered the campsite: {GetLocationName()}!");

            return true;
        }

        private void HoldUpMap(string message)
        {
            Game1.player.completelyStopAnimatingOrDoingAction();

            Game1.MusicDuckTimer = 2000f;
            DelayedAction.playSoundAfterDelay("getNewSpecialItem", 750);

            Game1.player.faceDirection(2);
            Game1.player.freezePause = 4000;

            Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[3]
            {
                new FarmerSprite.AnimationFrame(57, 0),
                new FarmerSprite.AnimationFrame(57, 2500, secondaryArm: false, flip: false, delegate(Farmer who)
                {
                    TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
                    {
                        motion = new Vector2(0f, -0.1f)
                    };
                    sprite.CopyAppearanceFromItemId(CAMPSITE_MAP_ID);
                    Game1.currentLocation.temporarySprites.Add(sprite);
                }),
                new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false, delegate(Farmer who)
                {
                    Game1.drawObjectDialogue(message);
                }, behaviorAtEndOfFrame: true)
            });

            Game1.player.canMove = false;
        }


        public string GetItemTypeId()
        {
            return TypeDefinitionId;
        }

        public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            var texture = Campgrounds.modHelper.GameContent.Load<Texture2D>("Data/PeacefulEnd_Campgrounds/Campgrounds/Textures/Items");
            var sourceRectangle = new Rectangle(16, 0, 16, 16);
            spriteBatch.Draw(texture, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), sourceRectangle, color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
        }

        public bool ShouldDrawIcon()
        {
            return true;
        }

        public string getDescription()
        {
            var locationName = GetLocationName();
            if (string.IsNullOrEmpty(locationName) is false)
            {
                return $"A map to the {locationName}.";
            }
            return "A map to a campsite.";
        }

        private string GetLocationName()
        {
            string locationName = string.Empty;
            if (Game1.locationData.ContainsKey(CampgroundData.Id))
            {
                locationName = Game1.locationData[CampgroundData.Id].DisplayName;
            }

            return locationName;
        }

        public int maximumStackSize()
        {
            return 1;
        }

        public int addToStack(Item stack)
        {
            return 1;
        }

        public bool canStackWith(ISalable other)
        {
            return false;
        }

        public int sellToStorePrice(long specificPlayerID = -1L)
        {
            return -1;
        }

        public int salePrice(bool ignoreProfitMargins = false)
        {
            return 1000;
        }

        public bool appliesProfitMargins()
        {
            return false;
        }

        public bool CanBuyItem(Farmer farmer)
        {
            return true;
        }

        public bool IsInfiniteStock()
        {
            return true;
        }

        public ISalable GetSalableInstance()
        {
            return this;
        }

        public void FixStackSize()
        {

        }

        public void FixQuality()
        {

        }
    }
}
