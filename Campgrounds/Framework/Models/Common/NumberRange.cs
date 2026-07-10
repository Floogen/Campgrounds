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
    public class NumberRange : BaseModel
    {
        public int Min { get; set; }
        public int? Max { get; set; }

        public bool IsWithinRange(int value)
        {
            if (value < Min)
            {
                return false;
            }

            if (Max.HasValue && value > Max.Value)
            {
                return false;
            }

            return true;
        }

        public override (bool Result, string Error) IsValid()
        {
            if (Max.HasValue && Min > Max.Value)
            {
                return (false, "The \"Max\" property must be greater than \"Min\" if given.");
            }

            return (true, string.Empty);
        }
    }
}
