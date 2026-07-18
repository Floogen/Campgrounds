using Campgrounds.Framework.Models.Common;
using Campgrounds.Framework.Models.Data.Visitors;
using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace Campgrounds.Framework.Models.Data
{
    public class CampgroundData : BaseModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string PreviewTexturePath { get; set; }
        public float PreviewTextureScale { get; set; } = 4f;
        public Vector2? PlayerSpawnTile { get; set; }
        public Vector2? GuestSpawnTile { get; set; }
        public SizeModel MaxTentTileSize { get; set; }
        public string TravelScreenText { get; set; }

        public bool RequireVehicle { get; set; }
        public int TravelTimeInHours { get; set; }

        public bool ForceForageRefreshOnVisit { get; set; }


        /// <summary>
        /// Overrides the standard VillagerData dialogues for any matching NPC
        /// </summary>
        public List<VisitorDialogueOverride> DialogueOverrides { get; set; } = new List<VisitorDialogueOverride>();

        public string UnlockCondition { get; set; }
        public string UnlockHint { get; set; }

        /// <summary>
        /// If true, the campsite will be hidden from the CampListMenu until the player unlocks it (UnlockHint will be ignored).
        /// </summary>
        public bool HideUntilUnlocked { get; set; }

        public bool IsTentValid(Direction direction, CampingTentData campingTentData)
        {
            if (campingTentData is null)
            {
                return false;
            }

            var tentSize = campingTentData.GetTileSize(direction);
            if (MaxTentTileSize is not null && (tentSize.Height > MaxTentTileSize.Height || tentSize.Width > MaxTentTileSize.Width))
            {
                return false;
            }

            return true;
        }

        public bool IsUnlocked()
        {
            if (string.IsNullOrEmpty(UnlockCondition))
            {
                return true;
            }

            return GameStateQuery.CheckConditions(UnlockCondition);
        }

        public override (bool Result, string Error) IsValid()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return (false, "Missing the \"Id\" property!");
            }

            if (PlayerSpawnTile is null)
            {
                return (false, "Missing the \"PlayerSpawnTile\" property!");
            }

            if (GuestSpawnTile is null)
            {
                return (false, "Missing the \"GuestSpawnTile\" property!");
            }

            if (TravelTimeInHours < 0)
            {
                return (false, "The \"TravelTimeInHours\" can't be negative!");
            }
            else if (TravelTimeInHours >= 16)
            {
                return (false, "The \"TravelTimeInHours\" can't be greater than 16!");
            }

            if (MaxTentTileSize is not null && MaxTentTileSize.IsValid().Result is false)
            {
                return (false, $"Error with \"MaxTentTileSize\": {MaxTentTileSize.IsValid().Error}");
            }

            return (true, string.Empty);
        }
    }
}
