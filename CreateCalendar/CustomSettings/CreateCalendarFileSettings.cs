using System.Collections.Generic;

namespace CreateCalendar.CustomSettings
{
    internal class CreateCalendarFileSettings : IEventSettings
    {
        public OldAppointmentOptions OldAppointments { get; set; }
        public string IcsFolder { get; set; } = "Calendar";
        public string IcsFilename { get; set; } = "{0}";
        public string FormatShift { get; set; } = "{0}";
        public ExcelRosterSettings ExcelRoster { get; set; }
        public Dictionary<string, string> AppointmentKeyValues { get; set; }
    }

}
