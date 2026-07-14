using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Common
{
    public class SizeModel : BaseModel
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public override (bool Result, string Error) IsValid()
        {
            if (Width < 0)
            {
                return (false, "The \"Width\" property must be greater than 0.");
            }
            if (Height < 0)
            {
                return (false, "The \"Height\" property must be greater than 0.");
            }

            return (true, string.Empty);
        }
    }
}
