using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models
{
    public class CampgroundData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TravelScreenText { get; set; }

        public bool RequireVehicle { get; set; }
        public int TravelTimeInHours { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return false;
            }

            if (TravelTimeInHours < 0)
            {
                return false;
            }

            return true;
        }
    }
}
