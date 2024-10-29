using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using CreateCalendar.Utilities;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CreateCalendar.ProcessXlCalendar
{
    internal class HeaderMapper
    {
        private readonly Dictionary<string, ColDetails> _colMapper;
        private readonly string[] _stringMapper;
        private readonly string _dateCol;
        private readonly string _maxCol;
        private static readonly Regex _beforeAfterBrcketMatcher = new Regex(@"^([^(]+)(\([^)]+\))?$");
        public IEnumerable<string> AllEmployees => (from cd in _colMapper.Values
                                                    where cd.ColType == ColType.ForEmployee
                                                    select cd.ColHeader);
        public List<ExcelCalDataRow> Roster { get; }
        public readonly HashSet<string> _ignoreShifts;
 
        public HeaderMapper(IEnumerable<Cell> employeeHeaders, IEnumerable<Cell> shiftsHeaders, string[] stringMapper,IExcelFileSettings settings)
        {
            _ignoreShifts = new HashSet<string>(
                settings.IgnoreShifts,
                StringComparer.OrdinalIgnoreCase
            )
            {
                string.Empty
            };
            _dateCol = settings.DateCol;
            _colMapper = [];

            if (!string.IsNullOrEmpty(settings.DateCommentsCol))
            {
                _colMapper.Add(settings.DateCommentsCol, new ColDetails
                {
                    ColType = ColType.DateComment
                }); ;
            }
            if (employeeHeaders != null)
            {
                foreach (var e in employeeHeaders)
                {
                    if (e.TryGetString(stringMapper, out string txt))
                    {
                        _colMapper.Add(e.ColumnReference(), new ColDetails
                        {
                            ColType = ColType.ForEmployee,
                            ColHeader = txt
                        });
                    }
                }
            }
            if (shiftsHeaders != null)
            {
                foreach (var s in shiftsHeaders)
                {
                    if (s.TryGetString(stringMapper, out string txt))
                    {
                        _colMapper.Add(s.ColumnReference(), new ColDetails
                        {
                            ColType = ColType.ForShifts,
                            ColHeader = txt
                        });
                    }
                }
            }
            _maxCol = _colMapper.Keys.MaxBy(ColLetterComparer.Instance);
            Roster = [];
            _stringMapper = stringMapper;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns>true if a date was found and the row was added</returns>
        public bool AddRow(Row row)
        {
            ExcelCalDataRow newCalDataRow = null;
            var cellEnumerator = row.ChildElements.OfType<Cell>().GetEnumerator();
            while (cellEnumerator.MoveNext())
            {
                string currentCol = cellEnumerator.Current.ColumnReference();
                var compare = ColLetterComparer.Instance.Compare(currentCol, _dateCol);
                if (compare == 0)
                {
                    if (cellEnumerator.Current.TryGetDateOnly(out DateOnly dt))
                    {
                        newCalDataRow = new ExcelCalDataRow
                        {
                            Date = dt,
                            Shifts = []
                        };
                        break;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (compare > 0)
                {
                    return false;
                }
            }
            while (cellEnumerator.MoveNext())
            {
                string currentCol = cellEnumerator.Current.ColumnReference();
                var compare = ColLetterComparer.Instance.Compare(currentCol, _maxCol);
                if (compare > 0)
                {
                    break;
                }
                if (cellEnumerator.Current.TryGetString(_stringMapper, out string str))
                {
                    str = str.Trim();
                    if (!_ignoreShifts.Contains(str) && _colMapper.TryGetValue(currentCol, out ColDetails cd))
                    {
                        switch (cd.ColType)
                        {
                            case ColType.DateComment:
                                newCalDataRow.DateComment = str;
                                break;
                            case ColType.ForShifts:
                                // hack here - needs to go in some kind of appsettings property
                                var inBrackets = _beforeAfterBrcketMatcher.Match(str);
                                if (inBrackets.Groups.Count > 2)
                                {
                                    newCalDataRow.Shifts.Add(new EmployeeShift
                                    {
                                        ShiftName = cd.ColHeader + inBrackets.Groups[2].Value,
                                        EmployeeName = inBrackets.Groups[1].Value.Trim(),
                                    });
                                }
                                else
                                {
                                    newCalDataRow.Shifts.Add(new EmployeeShift
                                    {
                                        ShiftName = cd.ColHeader,
                                        EmployeeName = str
                                    });
                                }
                                break;
                            case ColType.ForEmployee:
                                newCalDataRow.Shifts.Add(new EmployeeShift
                                {
                                    ShiftName = str,
                                    EmployeeName = cd.ColHeader
                                });
                                break;
                        }
                    }
                }
            }
            Roster.Add(newCalDataRow);
            return true;
        }
    }
}
