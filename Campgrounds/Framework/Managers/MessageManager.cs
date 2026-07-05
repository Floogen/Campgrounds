using Campgrounds.Framework.UI;
using Campgrounds.Framework.UI.Messages;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Managers
{
    public class MessageManager : BaseManager
    {
        public List<Message> Messages { get; set; } = new List<Message>();

        public MessageManager(IMonitor monitor, IModHelper helper) : base(monitor, helper)
        {
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            var expiredMessages = new List<Message>();
            foreach (var message in Messages)
            {
                if (message.Update() is false)
                {
                    expiredMessages.Add(message);
                }
            }

            Messages.RemoveAll(m => expiredMessages.Contains(m));
        }
    }
}
