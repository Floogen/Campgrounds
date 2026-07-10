using Campgrounds.Framework.Models.Common;
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

namespace Campgrounds.Framework.Models.Data.Visitors
{
    public class VisitorData : BaseModel
    {
        public string Id { get; set; }

        public List<DateModel> PreferredDates { get; set; }
        public List<DayOfWeek> PreferredDays { get; set; }

        public StandardVisitorSettings StandardVisitorSettings { get; set; }
        public MapVisitorSettings AdvancedVisitorSettings { get; set; }

        public string RequirementsCondition { get; set; }

        /// <summary>
        /// Returns if today is a valid date for this data. If both PreferredDates and PreferredDays are given, either one must contain the date / DayOfWeek to return true.
        /// </summary>
        /// <param name="today"></param>
        /// <returns></returns>
        public bool CanVisitToday(SDate today)
        {
            if (HasRequirements() is false)
            {
                return false;
            }


            if (PreferredDates is not null && PreferredDays is not null)
            {
                if (PreferredDates.Any(d => d.IsToday(today)) is true || PreferredDays.Contains(today.DayOfWeek) is true)
                {
                    return true;
                }

                return false;
            }
            else
            {
                if (PreferredDates is not null && PreferredDates.Any(d => d.IsToday(today)) is false)
                {
                    return false;
                }
                if (PreferredDays is not null && PreferredDays.Contains(today.DayOfWeek) is false)
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasPreferredDay(SDate date)
        {
            if (PreferredDays is null)
            {
                return false;
            }

            return PreferredDays.Any(d => d == date.DayOfWeek) is true;
        }

        public bool HasPreferredDate(SDate date)
        {
            if (PreferredDates is null)
            {
                return false;
            }

            return PreferredDates.Any(d => d.IsToday(date)) is true;
        }

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
            if (StandardVisitorSettings is not null && StandardVisitorSettings.IsValid().Result is false)
            {
                return (false, $"Error with the given StandardVisitorSettings: {StandardVisitorSettings.IsValid().Error}");
            }

            if (AdvancedVisitorSettings is not null && AdvancedVisitorSettings.IsValid().Result is false)
            {
                return (false, $"Error with the given AdvancedVisitorSettings: {AdvancedVisitorSettings.IsValid().Error}");
            }

            if (StandardVisitorSettings is null && AdvancedVisitorSettings is null)
            {
                return (false, $"Must give value for StandardVisitorSettings or AdvancedVisitorSettings");
            }
            if (StandardVisitorSettings is not null && AdvancedVisitorSettings is not null)
            {
                return (false, $"Values have been given for StandardVisitorSettings AND AdvancedVisitorSettings. You can only specify one!");
            }

            return (true, string.Empty);
        }
    }
}
