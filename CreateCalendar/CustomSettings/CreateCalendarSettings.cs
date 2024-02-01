using System.Collections.Generic;

namespace CreateCalendar.CustomSettings
{
    public class CreateCalendarSettings
    {
        public required string GoogleUser { get; set; }
        public List<CreateCalendarFileSettings> Calendars { get; set; } = [];
    }
}
