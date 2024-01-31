using System.Collections.Generic;

namespace CreateCalendar.CustomSettings
{
    public class CreateCalendarSettings
    {
        public string GoogleUser { get; set; }
        public List<CreateCalendarFileSettings> Calendars { get; set; } = [];
    }
}
