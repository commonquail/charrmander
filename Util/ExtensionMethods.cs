using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charrmander.Util
{
    public static class ExtensionMethods
    {
        public static bool IsNewerThan(this Version v, Version other)
        {
            return v.CompareTo(other) > 0;
        }
    }
}
