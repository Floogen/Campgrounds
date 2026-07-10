using Campgrounds.Framework.Models.Data.Visitors;
using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Common
{
    public class DateModel : BaseModel
    {
        public int Day { get; set; }

        /// <summary>
        /// Optional. If not given, IsToday will exclude it from the check.
        /// </summary>
        public Season? Season { get; set; }

        /// <summary>
        /// Optional. If not given, IsToday will exclude it from the check.
        /// </summary>
        public int? SpecificYear { get; set; }

        /// <summary>
        /// Optional. If YearRange.Max is not given, then all years on and after YearRange.Min will be included for IsToday.
        /// </summary>
        public NumberRange YearRange { get; set; }

        public bool IsToday(SDate today)
        {
            if (today.Day != Day)
            {
                return false;
            }

            if (Season is not null && today.Season != Season)
            {
                return false;
            }

            if (SpecificYear is not null && SpecificYear != today.Year)
            {
                return false;
            }
            if (YearRange is not null && YearRange.IsWithinRange(today.Year) is false)
            {
                return false;
            }

            return true;
        }

        public override (bool Result, string Error) IsValid()
        {
            if (Day < 1 || Day > 28)
            {
                return (false, "The \"Day\" property must be between 1 and 28 (inclusive).");
            }

            if (SpecificYear is not null && SpecificYear.Value < 1)
            {
                return (false, "The \"SpecificYear\" property must be greater than 1.");
            }

            if (YearRange is not null)
            {
                if (YearRange.IsValid().Result is false)
                {
                    return (false, $"Error with the given YearRange: {YearRange.IsValid().Error}");
                }

                if (YearRange.Min < 0)
                {
                    return (false, "The \"YearRange.Min\" property must be be >= 0.");
                }
                if (YearRange.Max < 0)
                {
                    return (false, "The \"YearRange.Max\" property must be >= 0.");
                }
            }

            return (true, string.Empty);
        }
    }
}
