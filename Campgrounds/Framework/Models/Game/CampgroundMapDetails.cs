using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Game
{
    public class CampgroundMapDetails : BaseModel
    {
        public Vector2? PlayerTentTile;
        public Vector2? GuestTentTile;

        public Direction PlayerTentDirection = Direction.South;
        public Direction GuestTentDirection = Direction.South;

        public Vector2? CookingSpotTile;

        public override (bool Result, string Error) IsValid()
        {
            return (true, string.Empty);
        }
    }
}
