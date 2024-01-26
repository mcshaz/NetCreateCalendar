using CreateCalendar.CustomSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.DataTransfer
{
    internal class ProcessedXlFileDetails
    {
        public CreateCalendarFileSettings Settings { get; set; }
        public EveryoneRoster Roster { get; set; }
        public Guid UniqueId { get; set; }
    }
}
