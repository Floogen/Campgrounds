using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Objects;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Managers
{
    public class ItemManager : BaseManager
    {
        public const string CAMPSITE_MAP_ID = "(O)PeacefulEnd.Campgrounds.Items.CampsiteMap";
        public const string CAMPFIRE_RECIPE_ID = "(O)PeacefulEnd.Campgrounds.Items.CampfireRecipe";

        public const string CAMPSITE_MAP_MOD_DATA_ID = "Campgrounds.Items.CampsiteMap.Data.Id";
        public const string CAMPFIRE_RECIPE_MOD_DATA_ID = "Campgrounds.Items.CampfireRecipe.Data.Id";

        public ItemManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.Player.InventoryChanged += OnInventoryChanged;
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            foreach (var item in e.Added)
            {
                if (IsCustomItem(item) is false)
                {
                    continue;
                }

                // Handle recipe-like items (campsite map, campfire food recipes, tent schematics)
                HandleCustomItem(e.Player, item);
            }
        }

        public void HandleCustomItem(Farmer farmer, Item item)
        {
            if (item.QualifiedItemId.EqualsIgnoreCase(CAMPSITE_MAP_ID))
            {
                if (item.modData.TryGetValue(CAMPSITE_MAP_MOD_DATA_ID, out string campgroundDataId))
                {
                    // Exit current menu
                    Game1.exitActiveMenu();

                    UnlockSpecialAndHoldAboveHead(GetCampsiteMapUnlockKey(campgroundDataId), CAMPSITE_MAP_ID, $"You have discovered the campsite: {Campgrounds.campManager.GetLocationNameFromDataId(campgroundDataId)}!");
                }

                farmer.removeItemFromInventory(item);
            }
            else if (item.QualifiedItemId.EqualsIgnoreCase(CAMPFIRE_RECIPE_ID))
            {
                if (item.modData.TryGetValue(CAMPFIRE_RECIPE_MOD_DATA_ID, out string campfireFoodDataId) && Campgrounds.campManager.GetCampfireFoodDataById(campfireFoodDataId) is CampfireFoodData campfireFoodData && campfireFoodData is not null)
                {
                    // Exit current menu
                    Game1.exitActiveMenu();

                    UnlockSpecialAndHoldAboveHead(GetCampfireRecipeUnlockKey(campfireFoodDataId), CAMPFIRE_RECIPE_ID, $"You have discovered the campfire cooking recipe: {campfireFoodData.DisplayName}!");
                }

                farmer.removeItemFromInventory(item);
            }
        }

        public bool IsCustomItem(Item item)
        {
            if (item.QualifiedItemId.EqualsIgnoreCase(CAMPSITE_MAP_ID) || item.QualifiedItemId.EqualsIgnoreCase(CAMPFIRE_RECIPE_ID))
            {
                return true;
            }

            return false;
        }

        public (bool Result, string Name) HasCustomName(Item item)
        {
            if (item is not null)
            {
                if (item.QualifiedItemId.EqualsIgnoreCase(CAMPSITE_MAP_ID) && item.modData.TryGetValue(CAMPSITE_MAP_MOD_DATA_ID, out string campgroundDataId))
                {
                    return (true, $"Campsite Map ({Campgrounds.campManager.GetLocationNameFromDataId(campgroundDataId)})");
                }
                if (item.QualifiedItemId.EqualsIgnoreCase(CAMPFIRE_RECIPE_ID) && item.modData.TryGetValue(CAMPFIRE_RECIPE_MOD_DATA_ID, out string campfireFoodDataId))
                {
                    var campfireFoodData = Campgrounds.campManager.GetCampfireFoodDataById(campfireFoodDataId);
                    if (campfireFoodData is not null)
                    {
                        return (true, $"Campfire Recipe ({campfireFoodData.DisplayName})");
                    }
                }
            }

            return (false, string.Empty);
        }

        public (bool Result, string Description) HasCustomDescription(Item item)
        {
            if (item is not null)
            {
                if (item.QualifiedItemId.EqualsIgnoreCase(CAMPSITE_MAP_ID) && item.modData.TryGetValue(CAMPSITE_MAP_MOD_DATA_ID, out string campgroundDataId))
                {
                    return (true, $"A map to the {Campgrounds.campManager.GetLocationNameFromDataId(campgroundDataId)}.");
                }
                if (item.QualifiedItemId.EqualsIgnoreCase(CAMPFIRE_RECIPE_ID) && item.modData.TryGetValue(CAMPFIRE_RECIPE_MOD_DATA_ID, out string campfireFoodDataId))
                {
                    var campfireFoodData = Campgrounds.campManager.GetCampfireFoodDataById(campfireFoodDataId);
                    if (campfireFoodData is not null)
                    {
                        return (true, $"A recipe for cooking {campfireFoodData.DisplayName} on a campfire.");
                    }
                }
            }

            return (false, string.Empty);
        }

        public string GetCampsiteMapUnlockKey(string campgroundDataId)
        {
            if (string.IsNullOrEmpty(campgroundDataId))
            {
                return string.Empty;
            }

            return $"MAP_UNLOCKED_CAMPGROUND:{campgroundDataId}";
        }

        public string GetCampfireRecipeUnlockKey(string campfireFoodDataId)
        {
            if (string.IsNullOrEmpty(campfireFoodDataId))
            {
                return string.Empty;
            }

            return $"RECIPE_UNLOCKED_CAMPFIRE_FOOD:{campfireFoodDataId}";
        }

        public void UnlockSpecialAndHoldAboveHead(string unlockKey, string itemId, string message)
        {
            // Unlock the associated item
            NetWorldState.addWorldStateIDEverywhere(unlockKey);

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
                    sprite.CopyAppearanceFromItemId(itemId);
                    Game1.currentLocation.temporarySprites.Add(sprite);
                }),
                new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false, delegate(Farmer who)
                {
                    Game1.drawObjectDialogue(message);
                }, behaviorAtEndOfFrame: true)
            });

            Game1.player.canMove = false;
        }
    }
}
