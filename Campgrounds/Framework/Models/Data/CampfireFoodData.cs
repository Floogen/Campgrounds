using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Data
{
    public class CampfireFoodData : BaseModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int RationCost { get; set; }

        public string TexturePath { get; set; }
        public Rectangle? SourceRectangle { get; set; }

        public List<string> BuffIds { get; set; } = new List<string>();

        public string UnlockCondition { get; set; }
        public string UnlockHint { get; set; }

        /// <summary>
        /// If true, the campsite will be hidden from the CampListMenu until the player unlocks it (UnlockHint will be ignored).
        /// </summary>
        public bool HideUntilUnlocked { get; set; }

        public List<Buff> GetBuffs()
        {
            List<Buff> buffs = new List<Buff>();
            foreach (var buffId in BuffIds)
            {
                buffs.Add(new Buff(buffId));
            }

            return buffs;
        }

        public bool IsUnlocked()
        {
            if (string.IsNullOrEmpty(UnlockCondition))
            {
                return true;
            }

            return GameStateQuery.CheckConditions(UnlockCondition);
        }

        public override (bool Result, string Error) IsValid()
        {
            if (string.IsNullOrEmpty(DisplayName))
            {
                return (false, "DisplayName needs to be set!");
            }

            if (RationCost < 0)
            {
                return (false, "RationCost must be greater or equal to 0!");
            }

            if (string.IsNullOrEmpty(TexturePath))
            {
                return (false, "Missing TexturePath!");
            }
            if (SourceRectangle is null)
            {
                return (false, "Missing SourceRectangle!");
            }

            return (true, string.Empty);
        }
    }
}
