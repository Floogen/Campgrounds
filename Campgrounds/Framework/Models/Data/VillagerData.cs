using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Data
{
    public class VillagerData
    {
        public string Id { get; set; }

        public string TentTexturePath { get; set; }
        public Rectangle TentBuildingRectangle { get; set; }

        public List<string> LikedCampgrounds { get; set; } = new List<string>();
        public List<string> DislikedCampgrounds { get; set; } = new List<string>();

        public List<string> LikedDialogue { get; set; } = new List<string>();
        public List<string> NeutralDialogue { get; set; } = new List<string>();
        public List<string> DislikedDialogue { get; set; } = new List<string>();


        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return false;
            }

            return true;
        }
    }
}
