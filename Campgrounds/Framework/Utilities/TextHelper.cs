using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Utilities
{
    public static class TextHelper
    {
        public static (string First, string Second) SplitLabel(string text, int maxLength = 16)
        {
            if (text.Length <= maxLength)
            {
                return (text, "");
            }

            int breakAt = text.LastIndexOf(' ', maxLength);
            if (breakAt <= 0)
            {
                breakAt = maxLength;
            }

            return (text.Substring(0, breakAt).TrimEnd(), text.Substring(breakAt).TrimStart());
        }
    }
}
