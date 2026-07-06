using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Data
{
    public class CampingTentData : BaseData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }

        public string TexturePath { get; set; }
        public DirectionalSprite NorthSprite { get; set; }
        public DirectionalSprite EastSprite { get; set; }
        public DirectionalSprite SouthSprite { get; set; }
        public DirectionalSprite WestSprite { get; set; }

        public List<string> RestingBuffIds { get; set; } = new List<string>();

        public int NumberOfAllowedCampfireMeals { get { return _numberOfAllowedCampfireMeals; } set { _numberOfAllowedCampfireMeals = Math.Min(5, Math.Max(value, 1)); } }
        private int _numberOfAllowedCampfireMeals = 1;

        public List<Buff> GetBuffs()
        {
            List<Buff> buffs = new List<Buff>();
            foreach (var buffId in RestingBuffIds)
            {
                buffs.Add(new Buff(buffId));
            }

            return buffs;
        }

        public override (bool Result, string Error) IsValid()
        {
            if (string.IsNullOrEmpty(DisplayName))
            {
                return (false, "DisplayName needs to be set!");
            }

            if (NorthSprite is null)
            {
                return (false, "Missing NorthSprite!");
            }

            if (EastSprite is null)
            {
                return (false, "Missing EastSprite!");
            }
            if (SouthSprite is null)
            {
                return (false, "Missing SouthSprite!");
            }
            if (WestSprite is null)
            {
                return (false, "Missing WestSprite!");
            }

            return (true, string.Empty);
        }
    }
}
