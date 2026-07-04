using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models
{
    public class DirectionalSprite
    {
        public Vector2 EntranceTile { get; set; }

        public Rectangle DisplayRectangle { get; set; }
        public Rectangle BoundaryRectangle { get; set; }

        public Rectangle ShadowRectangle { get; set; }
        public Vector2 ShadowOffset { get; set; }
    }
}
