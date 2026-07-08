using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Data
{
    public class VillagerData : BaseData
    {
        public string Id { get; set; }

        public string TentId { get; set; }

        public List<string> LikedCampgrounds { get; set; } = new List<string>();
        public List<string> DislikedCampgrounds { get; set; } = new List<string>();

        public List<string> LikedDialogue { get; set; } = new List<string>();
        public List<string> NeutralDialogue { get; set; } = new List<string>();
        public List<string> DislikedDialogue { get; set; } = new List<string>();


        public override (bool Result, string Error) IsValid()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return (false, "Id needs to be set!"); ;
            }

            return (true, string.Empty);
        }
    }
}
