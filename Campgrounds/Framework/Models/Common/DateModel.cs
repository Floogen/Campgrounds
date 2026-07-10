using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Common
{
    public class DateModel
    {
        public int Day { get; set; }
        public string Season { get; set; }

        public int Year { get; set; } = 1;
    }
}
