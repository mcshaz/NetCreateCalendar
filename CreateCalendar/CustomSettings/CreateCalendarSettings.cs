using System.Collections.Generic;

namespace CreateCalendar.CustomSettings
{
    internal class CreateCalendarSettings
    {
        public string GoogleUser { get; set; }
        public IList<CreateCalendarFileSettings> Calendars { get; set; }
    }
}
