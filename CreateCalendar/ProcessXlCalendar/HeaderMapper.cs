using ClosedXML.Excel;
using CreateCalendar.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateCalendar.ProcessXlCalendar
{
    internal class HeaderMapper
    {
        private readonly Dictionary<int, string> _colToEmployeeName;
        private readonly Dictionary<int, string> _colToSpecialShift;

        public IEnumerable<string> AllEmployees => _colToEmployeeName.Values;
        public List<ExcelCalDataRow> Roster { get; }
        public static HashSet<string> IgnoreShifts { get; set; } = new HashSet<string>(new[] { "PH", "RDO" });
 
        public HeaderMapper(IEnumerable<IXLCell> employeeCells, IEnumerable<IXLCell> specialShifts)
        {
            Roster = new List<ExcelCalDataRow>();
            _colToEmployeeName = new Dictionary<int, string>();
            foreach (var cell in employeeCells)
            {
                if (cell.TryGetValue(out string name))
                {
                    _colToEmployeeName.Add(cell.Address.ColumnNumber, name.Trim());
                }
            }
            _colToSpecialShift = new Dictionary<int, string>();
            if (specialShifts != null)
            {
                foreach (var cell in specialShifts)
                {
                    if (cell.TryGetValue(out string name))
                    {
                        _colToSpecialShift.Add(cell.Address.ColumnNumber, name.Trim());
                    }
                }
            }
        }
        public void AddRow(IXLRow row)
        {
            var cells = row.CellsUsed();
            var newCalDataRow = new ExcelCalDataRow
            {
                Date = DateOnly.FromDateTime(cells.First().Value.GetDateTime()),
                Shifts = new List<EmployeeShift>()
            };
            foreach (var cell in cells.Skip(1))
            {
                if (cell.TryGetValue(out string cellVal) && !string.IsNullOrWhiteSpace(cellVal))
                {
                    cellVal = cellVal.Trim();
                    if (_colToEmployeeName.TryGetValue(cell.Address.ColumnNumber, out string employee))
                    {
                        if (!IgnoreShifts.Contains(cellVal))
                        {
                            newCalDataRow.Shifts.Add(new EmployeeShift
                            {
                                ShiftName = cellVal,
                                EmployeeName = employee
                            });
                        }
                    }
                    else if (_colToSpecialShift.TryGetValue(cell.Address.ColumnNumber, out string specialShift))
                    {
                        newCalDataRow.Shifts.Add(new EmployeeShift
                        {
                            ShiftName = specialShift,
                            EmployeeName = cellVal
                        });
                    }
                }
            }
            Roster.Add(newCalDataRow);
        }
    }
}
