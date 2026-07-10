using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Data.Visitors;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using Campgrounds.Framework.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;

namespace Campgrounds.Framework.Managers
{
    public class VisitorManager : BaseManager
    {
        public List<VisitorData> VisitorData { get { return _visitorData; } set { FilterVisitorData(value); } }
        private List<VisitorData> _visitorData = new List<VisitorData>();

        public Dictionary<VisitorSpots, VisitorData> ActiveVisitorSpots { get; set; } = new Dictionary<VisitorSpots, VisitorData>();

        public VisitorManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            VisitorData = helper.GameContent.Load<List<VisitorData>>(Campgrounds.PARK_VISITORS_DATA_PATH);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            ActiveVisitorSpots.Clear();

            // Get today's game date
            SDate today = SDate.Now();

            // Determine visitors for the three available spots in the park
            var visitorDataCopy = new List<VisitorData>(VisitorData);

            // Get site 1 (SW)
            var swParkSpotVisitorData = GetVisitorsForToday(visitorDataCopy, today, VisitorSpots.SW);
            if (swParkSpotVisitorData != null)
            {
                visitorDataCopy.Remove(swParkSpotVisitorData);
                ActiveVisitorSpots[VisitorSpots.SW] = swParkSpotVisitorData;
            }

            // Get site 2 (NW)
            var nwParkSpotVisitorData = GetVisitorsForToday(visitorDataCopy, today, VisitorSpots.NW);
            if (nwParkSpotVisitorData != null)
            {
                visitorDataCopy.Remove(nwParkSpotVisitorData);
                ActiveVisitorSpots[VisitorSpots.NW] = nwParkSpotVisitorData;
            }

            // Get site 2 (SE)
            var seParkSpotVisitorData = GetVisitorsForToday(visitorDataCopy, today, VisitorSpots.SE);
            if (seParkSpotVisitorData != null)
            {
                visitorDataCopy.Remove(seParkSpotVisitorData);
                ActiveVisitorSpots[VisitorSpots.SE] = seParkSpotVisitorData;
            }

            // Invalidate the park's map to force the patches to apply
            helper.GameContent.InvalidateCache(Campgrounds.CINDERSAP_PARK_MAP_PATH);
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(Campgrounds.CINDERSAP_PARK_MAP_PATH))
            {
                e.Edit(asset =>
                {
                    // Check which visitor campsite(s) have been unlocked
                    var editor = asset.AsMap();

                    // SW Visitor Campsite
                    if (NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(1)) is true && ActiveVisitorSpots.ContainsKey(VisitorSpots.SW))
                    {
                        var visitorData = ActiveVisitorSpots[VisitorSpots.SW];
                        if (visitorData.AdvancedVisitorSettings != null)
                        {
                            foreach (var mapPatch in visitorData.AdvancedVisitorSettings.MapPatches)
                            {
                                editor.PatchMap(
                                    source: helper.GameContent.Load<Map>(mapPatch.MapPath),
                                    sourceArea: mapPatch.FromArea,
                                    targetArea: mapPatch.ToArea,
                                    patchMode: mapPatch.PatchMode
                                );
                            }
                        }
                    }

                    if (NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(2)) is true && ActiveVisitorSpots.ContainsKey(VisitorSpots.NW))
                    {
                        var visitorData = ActiveVisitorSpots[VisitorSpots.NW];
                        if (visitorData.AdvancedVisitorSettings != null)
                        {
                            foreach (var mapPatch in visitorData.AdvancedVisitorSettings.MapPatches)
                            {
                                editor.PatchMap(
                                    source: helper.GameContent.Load<Map>(mapPatch.MapPath),
                                    sourceArea: mapPatch.FromArea,
                                    targetArea: mapPatch.ToArea,
                                    patchMode: mapPatch.PatchMode
                                );
                            }
                        }
                    }

                    if (NetWorldState.checkAnywhereForWorldStateID(CampingHelper.GetCindersapParkVisitorParkKey(3)) is true && ActiveVisitorSpots.ContainsKey(VisitorSpots.SE))
                    {
                        var visitorData = ActiveVisitorSpots[VisitorSpots.SE];
                        if (visitorData.AdvancedVisitorSettings != null)
                        {
                            foreach (var mapPatch in visitorData.AdvancedVisitorSettings.MapPatches)
                            {
                                editor.PatchMap(
                                    source: helper.GameContent.Load<Map>(mapPatch.MapPath),
                                    sourceArea: mapPatch.FromArea,
                                    targetArea: mapPatch.ToArea,
                                    patchMode: mapPatch.PatchMode
                                );
                            }
                        }
                    }
                });
            }
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

            _visitorData = visitorData.Where(d => d.IsValid().Result is true).ToList();
        }

        public IEnumerable<VisitorData> GetPreferredDateVisitorData(IEnumerable<VisitorData> visitors, SDate date, VisitorSpots visitorSpot)
        {
            var allPreferredDateVisitors = VisitorData
                .Where(d => d.HasPreferredDate(date))
                .Where(d => d.StandardVisitorSettings is not null || (d.AdvancedVisitorSettings is not null && d.AdvancedVisitorSettings.RequiredSpot == visitorSpot));

            return allPreferredDateVisitors;
        }

        public IEnumerable<VisitorData> GetPreferredDayVisitorData(IEnumerable<VisitorData> visitors, SDate date, VisitorSpots visitorSpot)
        {
            var allPreferredDateVisitors = VisitorData
                .Where(d => d.HasPreferredDay(date))
                .Where(d => d.StandardVisitorSettings is not null || (d.AdvancedVisitorSettings is not null && d.AdvancedVisitorSettings.RequiredSpot == visitorSpot));

            return allPreferredDateVisitors;
        }

        public VisitorData GetRandomVisitorForFlexibleSpot(IEnumerable<VisitorData> visitors)
        {
            return GetRandomVisitor(visitors.Where(d => d.StandardVisitorSettings is not null));
        }

        public VisitorData GetVisitorsForToday(IEnumerable<VisitorData> visitors, SDate today, VisitorSpots visitorSpot)
        {
            var swParkSpotVisitorData = GetRandomVisitor(GetPreferredDateVisitorData(visitors, today, visitorSpot));
            if (swParkSpotVisitorData is null)
            {
                swParkSpotVisitorData = GetRandomVisitor(GetPreferredDayVisitorData(visitors, today, visitorSpot));

                if (swParkSpotVisitorData is null)
                {
                    swParkSpotVisitorData = GetRandomVisitorForFlexibleSpot(visitors);
                }
            }

            return swParkSpotVisitorData;
        }

        private VisitorData GetRandomVisitor(IEnumerable<VisitorData> visitors)
        {
            SDate today = SDate.Now();
            VisitorData visitorData = null;

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

            // Lastly check for any that CanVisitToday
            visitorData = visitors.OrderBy(x => Game1.random.Next()).FirstOrDefault(d => d.CanVisitToday(today));
            if (visitorData is not null)
            {
                return visitorData;
            }

            return visitorData;
        }
    }
}
