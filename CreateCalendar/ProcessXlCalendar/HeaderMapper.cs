using ClosedXML.Excel;
using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateCalendar.ProcessXlCalendar
{
    internal class HeaderMapper
    {
        private readonly IEnumerable<KeyValuePair<int, string>> _colToEmployeeName;
        private readonly IEnumerable<KeyValuePair<int, string>> _colToSpecialShift;
        private readonly int _dateCol;
        private readonly int _dateCommentCol;
        public IEnumerable<string> AllEmployees => _colToEmployeeName.Select(kv => kv.Value);
        public List<ExcelCalDataRow> Roster { get; }
        public readonly HashSet<string> _ignoreShifts;
 
        public HeaderMapper(IEnumerable<IXLCell> employeeCells, IEnumerable<IXLCell> specialShifts, IExcelFileSettings settings)
        {
            _ignoreShifts = new HashSet<string>(
                settings.IgnoreShifts, 
                StringComparer.OrdinalIgnoreCase
            );
            _dateCol = settings.DateCol;
            _dateCommentCol = settings.DateCommentsCol ?? -1;
            Roster = new List<ExcelCalDataRow>();
            var colToEmployeeName = new List<KeyValuePair<int, string>>(employeeCells.Count());
            foreach (var cell in employeeCells)
            {
                if (cell.TryGetValue(out string name))
                {
                    colToEmployeeName.Add(new KeyValuePair<int, string>(cell.Address.ColumnNumber, name.Trim()));
                }
            }
            _colToEmployeeName = colToEmployeeName;
            if (specialShifts == null)
            {
                _colToSpecialShift = Enumerable.Empty<KeyValuePair<int, string>>();
            }
            else 
            {
                var colToSpecialShift = new List<KeyValuePair<int, string>>(specialShifts.Count());
                foreach (var cell in specialShifts)
                {
                    if (cell.TryGetValue(out string name))
                    {
                        colToSpecialShift.Add(new KeyValuePair<int, string>(cell.Address.ColumnNumber, name.Trim()));
                    }
                }
                _colToSpecialShift = colToSpecialShift;
            }
        }
        public void AddRow(IXLRow row)
        {
            if (!row.Cell(_dateCol).TryGetValue(out DateTime dt))
            {
                return;
            }
            var newCalDataRow = new ExcelCalDataRow
            {
                Date = DateOnly.FromDateTime(dt),
                Shifts = new List<EmployeeShift>()
            };
            if (row.Cell(_dateCommentCol).TryGetValue(out string commentVal) && !string.IsNullOrWhiteSpace(commentVal))
            {
                newCalDataRow.DateComment = commentVal.Trim();
            }
            foreach (var kv in _colToEmployeeName)
            {
                if (row.Cell(kv.Key).TryGetValue(out string shiftName) && !string.IsNullOrWhiteSpace(shiftName))
                {
                    shiftName = shiftName.Trim();
                    if (!_ignoreShifts.Contains(shiftName))
                    {
                        newCalDataRow.Shifts.Add(new EmployeeShift
                        {
                            ShiftName = shiftName.Trim(),
                            EmployeeName = kv.Value
                        });
                    }
                }
            }
            foreach (var kv in _colToSpecialShift)
            {
                if (row.Cell(kv.Key).TryGetValue(out string personDoingShift) && !string.IsNullOrWhiteSpace(personDoingShift))
                {
                    newCalDataRow.Shifts.Add(new EmployeeShift
                    {
                        ShiftName = kv.Value,
                        EmployeeName = personDoingShift.Trim()
                    });
                }
            }
            Roster.Add(newCalDataRow);
        }
    }
}
