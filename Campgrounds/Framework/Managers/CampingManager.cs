using Campgrounds.Framework.Models;
using Campgrounds.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Managers
{
    public class CampingManager : BaseManager
    {
        public List<CampgroundData> CampgroundData { get; set; } = new List<CampgroundData>();

        public bool IsTraveling { get; private set; }
        private TravelMessage _travelMessage;

        public CampingManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            CampgroundData = helper.GameContent.Load<List<CampgroundData>>(Campgrounds.CAMPGROUND_DATA_PATH);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (_travelMessage is not null)
            {
                _travelMessage.Update();
            }
        }

        public void StartTraveling(CampgroundData campgroundData)
        {
            if (IsTraveling is true)
            {
                return;
            }
            IsTraveling = true;

            _travelMessage = new TravelMessage(campgroundData);
        }

        public void StopTraveling()
        {
            IsTraveling = false;
            _travelMessage = null;
        }
    }
}
