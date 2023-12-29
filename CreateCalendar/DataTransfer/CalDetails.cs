using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.DataTransfer
{
    internal class CalDetails
    {
        public DateTime LastUpdated { get; set; }
        public int Sequence { get; set; }
        public List<ApptDetails> Appointments { get; set; }
    }
    internal class ApptDetails 
    {
        public string Uid { get; set;}
        public bool IsCancelled { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> Content { get; set; }
    }
}
