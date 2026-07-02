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

        public (bool Result, string Error) IsValid()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return (false, "Missing the \"Name\" property!");
            }

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
