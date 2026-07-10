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
        public List<MapPatchModel> MapPatches { get; set; }

        public override (bool Result, string Error) IsValid()
        {
            if (RequiredSpot is null)
            {
                return (false, "Missing the \"RequiredSpot\" property!");
            }

            if (MapPatches is null || MapPatches.Count == 0)
            {
                return (false, "Missing the \"MapPatchId\" property!");
            }

            int mapPatchCount = 1;
            foreach (var mapPatch in MapPatches)
            {
                if (mapPatch.IsValid().Result is false)
                {
                    return (false, $"Error with MapPatch #{mapPatchCount}: {mapPatch.IsValid().Error}");
                }

                mapPatchCount += 1;
            }

            return (true, string.Empty);
        }
    }
}
