using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Models.Data
{
    public abstract class BaseData
    {
        public abstract (bool Result, string Error) IsValid();
    }
}
