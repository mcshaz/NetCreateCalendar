using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.ProcessXlCalendar
{
    internal enum ColType
    {
        DateComment,
        ForEmployee,
        ForShifts
    }
    internal class ColDetails
    {
        public ColType ColType { get; set; }
        public string ColHeader { get; set; }

    }
}
