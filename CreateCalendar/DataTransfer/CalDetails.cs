using CreateCalendar.CreateIcs;
using CreateCalendar.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CreateCalendar.DataTransfer
{
    internal class ApptDetails 
    {
        private bool _isCancelled;
        private int _sequence = -1;
        private DateTime _lastModified;
        private DateTime _dtStamp;
        private string _summary;

        private int _lastModifiedLineIndex = -1;
        private int _sequenceLineIndex = -1;
        private int _dtStampLineIndex = -1;
        private int _summaryLineIndex = -1;
        private int _statusLineIndex = -1;

        private readonly List<string> _content = new List<string>();

        public const string EndApptLine = "END:VEVENT";
        public const string UidPrefix = "UID:";
        public const string LastModifiedPrefix = "LAST-MODIFIED:";
        public const string DtStampPrefix = "DTSTAMP:";
        public const string StatusPrefix = "STATUS:";
        public const string SequencePrefix = "SEQUENCE:";
        public const string CreatedPrefix = "CREATED:";
        public const string SummaryPrefix = "SUMMARY:";

        public const string CancelledSummaryPrefix = "Cancelled:";
        // TODO - might need to adjust line folding here as summary might exceed length
        public string Summary { 
            get => _summary; 
            set {
                _summary = value;
                var newSummary = SummaryPrefix + value;
                if (_summaryLineIndex == -1)
                {
                    // could throw here - summary should be part of vevent
                    _content[_content.Count - 1] = newSummary;
                    _content.Add(EndApptLine);
                }
                else
                {
                    _content[_summaryLineIndex] = newSummary;
                }
            }
        }
        public string Uid { get; private set;}
        public DateTime Created { get; private set; }
        public bool IsCancelled { 
            get => _isCancelled;
            set { 
                _isCancelled = value;
                var newStatus = StatusPrefix;
                if (_isCancelled)
                {
                    newStatus += "CANCELLED";
                    if (!Summary.StartsWith(CancelledSummaryPrefix))
                    {
                        Summary = CancelledSummaryPrefix + Summary;
                    }
                }
                else
                {
                    newStatus += "CONFIRMED";
                    if (Summary.StartsWith(CancelledSummaryPrefix))
                    {
                        Summary = Summary.Substring(CancelledSummaryPrefix.Length);
                    }
                }
                if (_statusLineIndex == -1)
                {
                    _content[_content.Count - 1] = newStatus;
                    _content.Add(EndApptLine);
                }
                else 
                {
                    _content[_statusLineIndex] = newStatus;
                }

            } 
        }
        public DateTime LastModified { 
            get => _lastModified; 
            set
            {
                _lastModified = value;
                var newLastModified = LastModifiedPrefix + IcsDateHelpers.IcsDateFormat(value);
                if (_lastModifiedLineIndex == -1)
                {
                    _content[_content.Count - 1] = newLastModified;
                    _content.Add(EndApptLine);
                }
                else
                {
                    _content[_lastModifiedLineIndex] = newLastModified;
                }
            }
        }
        public DateTime DtStamp
        {
            get => _dtStamp;
            set
            {
                _dtStamp = value;
                var newDtStamp = DtStampPrefix + IcsDateHelpers.IcsDateFormat(value);
                if (_dtStampLineIndex == -1)
                {
                    _content[_content.Count - 1] = newDtStamp;
                    _content.Add(EndApptLine);
                }
                else
                {
                    _content[_dtStampLineIndex] = newDtStamp;
                }
            }
        }
        public int Sequence
        {
            get => _sequence;
            set
            {
                _sequence = value;
                var newSequence = SequencePrefix + value;
                if (_sequenceLineIndex == -1)
                {
                    _content[_content.Count - 1] = newSequence;
                    _content.Add(EndApptLine);
                }
                else
                {
                    _content[_sequenceLineIndex] = newSequence;
                }
            }
        }
        public IEnumerable<string> Content { get => _content; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line">Should be a line from an ics file within a VEVENT block (including begin & end)</param>
        /// <returns>False if the VEVENT terminator has been reached</returns>
        public bool AddLine(string line)
        {
            _content.Add(line);
            if (line == EndApptLine)
            {
                return false;
            }
            if (line.StartsWith(UidPrefix))
            {
                Uid = line.Substring(UidPrefix.Length);
            }
            else if (line.StartsWith(SummaryPrefix))
            {
                _summary = line.Substring(SummaryPrefix.Length);
                _summaryLineIndex = _content.Count - 1;
            }
            else if (line.StartsWith(LastModifiedPrefix))
            {
                _lastModified = ICalDtTimeToNet(line.Substring(LastModifiedPrefix.Length));
                _lastModifiedLineIndex = _content.Count - 1;
            }
            else if (line.StartsWith(CreatedPrefix))
            {
                Created = ICalDtTimeToNet(line.Substring(CreatedPrefix.Length));
            }
            else if (line.StartsWith(DtStampPrefix))
            {
                _dtStamp = ICalDtTimeToNet(line.Substring(DtStampPrefix.Length));
                _dtStampLineIndex = _content.Count - 1;
            }
            else if (line.StartsWith(StatusPrefix))
            {
                _isCancelled = line.Substring(StatusPrefix.Length) == "CANCELLED";
                _statusLineIndex = _content.Count - 1;
            }
            else if (line.StartsWith(SequencePrefix))
            {
                var seq = int.Parse(line.Substring(SequencePrefix.Length));
                _sequence = seq;
                _sequenceLineIndex = _content.Count - 1;
            }
            return true;
        }
        public async Task Write(CalendarWriter cw)
        {
            for (int i = 0; i < _content.Count; i++)
            {
                await cw.WriteLineRaw(_content[i]);
            }
        }
        public static DateTime ICalDtTimeToNet(string icalDtTime)
        {
            return new DateTime(
                int.Parse(icalDtTime.Substring(0, 4)),
                int.Parse(icalDtTime.Substring(4, 2)),
                int.Parse(icalDtTime.Substring(6, 2)),
                int.Parse(icalDtTime.Substring(9, 2)),
                int.Parse(icalDtTime.Substring(11, 2)),
                int.Parse(icalDtTime.Substring(13, 2)),
                icalDtTime.Length >= 16 && icalDtTime[15] == 'Z'
                    ? DateTimeKind.Utc
                    : DateTimeKind.Local // local to the user, not necessarily system datetime, but it is a console app so very likely to be the local time of the machine it is running on
            );
        }
    }
}
