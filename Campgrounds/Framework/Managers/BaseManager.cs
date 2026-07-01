using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Managers
{
    public class BaseManager
    {
        internal IModHelper helper;

        public BaseManager(IModHelper helper)
        {
            this.helper = helper;
        }
    }
}
