using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
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
    public class MapPatchModel : BaseModel
    {
        public string MapPath { get; set; }
        public Rectangle? FromArea { get; set; }
        public Rectangle? ToArea { get; set; }
        public PatchMapMode PatchMode { get; set; } = PatchMapMode.Overlay;

        public override (bool Result, string Error) IsValid()
        {
            if (string.IsNullOrEmpty(MapPath))
            {
                return (false, "The \"MapPath\" property must be given.");
            }

            return (true, string.Empty);
        }
    }
}
