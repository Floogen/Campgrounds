using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Data.Visitors;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Managers
{
    public class VisitorManager : BaseManager
    {
        public List<VisitorData> VisitorData { get { return _visitorData; } set { FilterVisitorData(value); } }
        private List<VisitorData> _visitorData = new List<VisitorData>();

        public VisitorManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {

        }

        private void FilterVisitorData(List<VisitorData> visitorData)
        {
            foreach (var visitors in visitorData)
            {
                var isValidData = visitors.IsValid();
                if (isValidData.Result is false)
                {
                    monitor.LogOnce($"Skipping invalid VisitorData with name \"{visitors.Id}\": {isValidData.Error}", LogLevel.Warn);
                }
            }

            _visitorData = visitorData.Where(c => c.IsValid().Result is true).ToList();
        }

        public VisitorData GetRandomVisitorForAnySpot()
        {
            return GetRandomVisitor(VisitorData.Where(d => d.StandardVisitorSettings is not null));
        }

        public VisitorData GetRandomVisitorForSpecificSpot(VisitorSpots visitorSpot)
        {
            return GetRandomVisitor(VisitorData.Where(d => d.AdvancedVisitorSettings is not null && d.AdvancedVisitorSettings.RequiredSpot == visitorSpot));
        }

        private VisitorData GetRandomVisitor(IEnumerable<VisitorData> visitors = null)
        {
            SDate today = SDate.Now();
            VisitorData visitorData = null;

            if (visitors is null)
            {
                visitors = VisitorData;
            }

            // Check those with PreferredDates first
            visitorData = visitors.Where(d => d.PreferredDates is not null).OrderBy(x => Game1.random.Next()).FirstOrDefault(d => d.CanVisitToday(today));
            if (visitorData is not null)
            {
                return visitorData;
            }

            // Check those with PreferredDays next
            visitorData = visitors.Where(d => d.PreferredDays is not null).OrderBy(x => Game1.random.Next()).FirstOrDefault(d => d.CanVisitToday(today));
            if (visitorData is not null)
            {
                return visitorData;
            }

            // Lastly check any
            visitorData = visitors.OrderBy(x => Game1.random.Next()).FirstOrDefault(d => d.CanVisitToday(today));
            if (visitorData is not null)
            {
                return visitorData;
            }

            return visitorData;
        }
    }
}
