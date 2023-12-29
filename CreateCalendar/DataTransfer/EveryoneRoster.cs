using System.Collections.Generic;

namespace CreateCalendar.DataTransfer
{
    public class EveryoneRoster
    {
        public IEnumerable<ExcelCalDataRow> DailyRoster { get; set; }
        public IEnumerable<string> Employees { get; set; }
    }
}
