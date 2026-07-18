using Campgrounds.Framework.Models.Enums;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Data.Visitors
{
    public class VisitorDialogueOverride : BaseModel
    {
        public string Id { get; set; }

        public List<string> DialogueDayOfOverride { get; set; } = new List<string>();
        public List<string> DialogueDayAfterOverride { get; set; } = new List<string>();

        public bool HasOverride(CampDialogue campDialogue, string npcName)
        {
            if (Id.EqualsIgnoreCase(npcName) is false)
            {
                return false;
            }

            if (campDialogue is CampDialogue.LikedDayOf or CampDialogue.DislikedDayOf or CampDialogue.NeutralDayOf && DialogueDayOfOverride.Count > 0)
            {
                return true;
            }
            if (campDialogue is CampDialogue.LikedDayAfter or CampDialogue.DislikedDayAfter or CampDialogue.NeutralDayAfter && DialogueDayAfterOverride.Count > 0)
            {
                return true;
            }

            return false;
        }

        public override (bool Result, string Error) IsValid()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return (false, "Missing the \"Id\" property!");
            }

            return (true, string.Empty);
        }
    }
}
