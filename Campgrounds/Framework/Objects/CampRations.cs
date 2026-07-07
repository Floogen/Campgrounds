using Campgrounds.Framework.Managers;
using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static StardewValley.Minigames.CraneGame;

namespace Campgrounds.Framework.Objects
{
    public class CampRations : ISalable
    {
        public string TypeDefinitionId => "(Salable)";

        public string QualifiedItemId => TypeDefinitionId + "CampRations";

        public string DisplayName => "Camp Rations";

        public string Name => CurrencyManager.CAMP_RATION_CURRENCY_ID;

        public bool IsRecipe { get { return false; } set { } }
        public int Stack { get { return 1; } set { } }
        public int Quality { get { return 0; } set { } }

        public bool actionWhenPurchased(string shopId)
        {
            Campgrounds.currencyManager.ChangeCurrencyBalance(Currency.CampRations, Stack);
            return true;
        }

        public string GetItemTypeId()
        {
            return TypeDefinitionId;
        }

        public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(Campgrounds.currencyManager.GetTexture(), location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), Campgrounds.currencyManager.GetSourceRectangle(Currency.CampRations), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
        }

        public bool ShouldDrawIcon()
        {
            return true;
        }

        public string getDescription()
        {
            return "An assortment of cooking ingredients. Useful while camping out in the wilderness.";
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
            return 150;
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
