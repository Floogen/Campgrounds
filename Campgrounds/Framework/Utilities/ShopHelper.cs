using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace Campgrounds.Framework.Utilities
{
    public static class ShopHelper
    {
        public const int RECIPE_SHOP_SEED = 7172026;

        public static List<Object> GetDailyUnknownCookingRecipes(Farmer who, int maxCount)
        {
            Dictionary<string, string> cookingRecipes = DataLoader.CookingRecipes(Game1.content);

            List<string> recipeNames = cookingRecipes.Keys.OrderBy(recipeName => recipeName, StringComparer.Ordinal).ToList();
            Utility.Shuffle(Utility.CreateDaySaveRandom(RECIPE_SHOP_SEED), recipeNames);

            List<Object> results = new List<Object>();
            foreach (string recipeName in recipeNames)
            {
                if (results.Count >= maxCount)
                {
                    break;
                }
                if (who.knowsRecipe(recipeName))
                {
                    //continue;
                }

                string[] fields = cookingRecipes[recipeName].Split('/');
                string yieldItemId = fields[2].Split(' ')[0];

                Object recipeObject = new Object(yieldItemId, 1, isRecipe: true, price: -1, quality: 0);
                recipeObject.Name = recipeName;

                results.Add(recipeObject);
            }

            return results;
        }
    }
}
