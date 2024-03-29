﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace CreateCalendar.DataTransfer
{
    public class PerUserRosterDay
    {
        public virtual DateOnly Date { get; set; }
        public string DateComment { get; set; }
        public string Shift { get; set; }
        public IEnumerable<EmployeeShift> OtherEmployeesAvailable { get; set; }
    }
}
