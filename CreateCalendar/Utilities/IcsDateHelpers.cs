using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.Utilities
{
    internal static class IcsDateHelpers
    {
        public static string IcsDateFormat(DateTime dt)
        {
            var dtString = dt.ToString("yyyyMMddThhmmss");
            if (dt.Kind == DateTimeKind.Utc)
            {
                return dtString + 'Z';
            }
            else
            {
                return dtString;
            }
        }
    }
}
