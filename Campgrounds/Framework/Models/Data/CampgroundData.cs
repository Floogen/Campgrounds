using Microsoft.Xna.Framework;
using StardewValley;

namespace Campgrounds.Framework.Models.Data
{
    public class CampgroundData : BaseData
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string PreviewTexturePath { get; set; }
        public float PreviewTextureScale { get; set; } = 4f;
        public Vector2? PlayerSpawnTile { get; set; }
        public Vector2? GuestSpawnTile { get; set; }
        public string TravelScreenText { get; set; }

        public bool RequireVehicle { get; set; }
        public int TravelTimeInHours { get; set; }

        public bool ForceForageRefreshOnVisit { get; set; }

        public string UnlockCondition { get; set; }
        public string UnlockHint { get; set; }

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

            return (true, string.Empty);
        }
    }
}
