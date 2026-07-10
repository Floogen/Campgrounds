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
    public class StandardVisitorSettings : BaseModel
    {
        public List<string> Visitors { get; set; } = new List<string>();

        public override (bool Result, string Error) IsValid()
        {
            if (Visitors is null || Visitors.Count == 0)
            {
                return (false, "Missing the \"Visitors\" property!");
            }

            return (true, string.Empty);
        }
    }
}
