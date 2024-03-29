﻿using System.Collections.Generic;

namespace CreateCalendar.CustomSettings
{
    public interface IEventSettings
    {
        string IcsFilename { get; }
        string FormatShift { get;  }
        OldAppointmentOptions OldAppointments { get; }
        Dictionary<string, string> AppointmentKeyValues { get; }
    }
}
