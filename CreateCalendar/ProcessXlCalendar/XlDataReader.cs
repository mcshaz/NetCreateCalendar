using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using CreateCalendar.Utilities;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CreateCalendar.ProcessXlCalendar
{
    public class XlDataReader
    {
        // private readonly ILogger _logger;
        private readonly IExcelFileSettings _excelFileSettings;
        public XlDataReader(IExcelFileSettings excelFileSettings /*ILogger<XlDataReader> logger */)
        {
            _excelFileSettings = excelFileSettings;
            _sheetselector = new Regex(excelFileSettings.SheetnamePattern);
        }

        private static string[] GetIdToStringMapper(SpreadsheetDocument doc)
        {
            SharedStringTablePart shareStringPart = doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
            return shareStringPart.SharedStringTable.Elements<SharedStringItem>()
                .Select(i => i.InnerText).ToArray();
        }
        private readonly Regex _sheetselector;
        public EveryoneRoster Process(Stream xlStream)
        {
            using (var excelWorkbook = SpreadsheetDocument.Open(xlStream, false))
            {
                var employees = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var dateShifts = new List<ExcelCalDataRow>();
                var wbPart = excelWorkbook.WorkbookPart;
                var borderFinder = new BorderFinder(wbPart);
                var stringMapper = GetIdToStringMapper(excelWorkbook);
                var allWs =  (from s in wbPart.Workbook.Descendants<Sheet>()
                              where _sheetselector.IsMatch(s.Name)
                              orderby s.Name
                              select ((WorksheetPart)wbPart.GetPartById(s.Id)).Worksheet);
                foreach (var ws in allWs)
                {
                    HeaderMapper headerMapper = null;
                    var mergeFinder = new MergedRangeFinder(ws);
                    using (var rowsEnumerator = ws.GetFirstChild<SheetData>().Elements<Row>().GetEnumerator())
                    {
                        while (rowsEnumerator.MoveNext())
                        {
                            var dataRow = rowsEnumerator.Current;
                            if (headerMapper == null)
                            {
                                foreach (var cell in dataRow.Elements<Cell>())
                                {
                                    if (ColLetterComparer.Instance.Compare(cell.ColumnReference(), _excelFileSettings.DateCol) > 0)
                                    { 
                                        if (cell.TryGetString(stringMapper, out string cellTxt))
                                        {
                                            if (cellTxt.IndexOf(_excelFileSettings.EmployeeNamesBeneath, StringComparison.OrdinalIgnoreCase) > -1)
                                            {
                                                var borderedCols = mergeFinder.IncludesCell(cell) ?? borderFinder.FindBorders(cell);
                                                if (!borderedCols.HasValue || !rowsEnumerator.MoveNext())
                                                {
                                                    throw new Exception("Cannot Identify borders defining Employee Names");
                                                }
                                                dataRow = rowsEnumerator.Current;
                                                var cellsBetween = borderedCols.Value.ApplyToRow(dataRow);
                                                var specialShifts = new List<Cell>();
                                                foreach (var c in borderedCols.Value.CellsRight(dataRow))
                                                {
                                                    if (c.TryGetString(stringMapper, out string txt))
                                                    {
                                                        if (_excelFileSettings.SpecialShiftHeaders.Contains(txt))
                                                        {
                                                            specialShifts.Add(c);
                                                        }
                                                    }
                                                }
                                                headerMapper = new HeaderMapper(cellsBetween, specialShifts, stringMapper, _excelFileSettings);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                headerMapper.AddRow(dataRow);
                            }
                        }
                    }
                    if (headerMapper != null)
                    {
                        dateShifts.AddRange(headerMapper.Roster);
                        foreach (var e in headerMapper.AllEmployees)
                            employees.Add(e);
                    }
                }
                dateShifts.Sort((a, b) => a.Date.CompareTo(b.Date));
                return new EveryoneRoster
                {
                    DailyRoster = dateShifts,
                    Employees = employees
                };
            }
        }
    }
}
