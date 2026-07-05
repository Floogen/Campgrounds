using Microsoft.Xna.Framework;
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
