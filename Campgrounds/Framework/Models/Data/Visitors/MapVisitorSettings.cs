using Campgrounds.Framework.Models.Common;
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
    public class MapVisitorSettings : BaseModel
    {
        public VisitorSpots? RequiredSpot { get; set; }
        public MapPatchModel MapPatch { get; set; }

        public override (bool Result, string Error) IsValid()
        {
            if (RequiredSpot is null)
            {
                return (false, "Missing the \"RequiredSpot\" property!");
            }

            if (MapPatch is null)
            {
                return (false, "Missing the \"MapPatchId\" property!");
            }
            else if (MapPatch.IsValid().Result is false)
            {
                return (false, $"Error with the given MapPatch: {MapPatch.IsValid().Error}");
            }

            return (true, string.Empty);
        }
    }
}
