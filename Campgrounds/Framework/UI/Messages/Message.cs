using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace Campgrounds.Framework.UI
{
    public abstract class Message
    {
        public abstract bool Update();

        public abstract void Draw(SpriteBatch b);
    }
}
