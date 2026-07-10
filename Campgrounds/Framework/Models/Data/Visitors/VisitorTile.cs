using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Data.Visitors
{
    public class VisitorTile
    {
        public Vector2 Tile { get; set; }
        public Direction Direction { get; set; } = Direction.North;
    }
}
