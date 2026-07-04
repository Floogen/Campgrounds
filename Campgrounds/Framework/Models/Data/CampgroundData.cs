using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Data
{
    public class CampgroundData
    {
        public string Name { get; set; }
        public string PreviewTexturePath { get; set; }
        public float PreviewTextureScale { get; set; } = 4f;
        public Vector2? PlayerSpawnTile { get; set; }
        public Vector2? NPCSpawnTile { get; set; }
        public string Description { get; set; }
        public string TravelScreenText { get; set; }

        public bool RequireVehicle { get; set; }
        public int TravelTimeInHours { get; set; }

        public (bool Result, string Error) IsValid()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return (false, "Missing the \"Name\" property!");
            }

            if (PlayerSpawnTile is null)
            {
                return (false, "Missing the \"PlayerSpawnTile\" property!");
            }

            if (NPCSpawnTile is null)
            {
                return (false, "Missing the \"NPCSpawnTile\" property!");
            }

            if (TravelTimeInHours < 0)
            {
                return (false, "The \"TravelTimeInHours\" can't be negative!");
            }
            else if (TravelTimeInHours >= 16)
            {
                return (false, "The \"TravelTimeInHours\" can't be greater than 16!");
            }

            return (true, string.Empty);
        }
    }
}
