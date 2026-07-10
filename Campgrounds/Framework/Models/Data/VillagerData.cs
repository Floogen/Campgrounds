using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Data
{
    public class VillagerData : BaseModel
    {
        public string Id { get; set; }

        public string TentId { get; set; }

        public List<string> LikedCampgrounds { get; set; } = new List<string>();
        public List<string> DislikedCampgrounds { get; set; } = new List<string>();

        public List<string> InviteDialogueAccepted { get; set; } = new List<string>();
        public List<string> InviteDialogueRejected { get; set; } = new List<string>();

        public List<string> LikedDialogueDayOf { get; set; } = new List<string>();
        public List<string> NeutralDialogueDayOf { get; set; } = new List<string>();
        public List<string> DislikedDialogueDayOf { get; set; } = new List<string>();

        public List<string> LikedDialogueDayAfter { get; set; } = new List<string>();
        public List<string> NeutralDialogueDayAfter { get; set; } = new List<string>();
        public List<string> DislikedDialogueDayAfter { get; set; } = new List<string>();

        public string RequirementsCondition { get; set; }

        public bool HasRequirements()
        {
            if (string.IsNullOrEmpty(RequirementsCondition))
            {
                return true;
            }

            return GameStateQuery.CheckConditions(RequirementsCondition);
        }


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
