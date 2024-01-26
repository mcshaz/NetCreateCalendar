using ClosedXML.Excel;
using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CreateCalendar.ProcessXlCalendar
{
    public class XlDataReader
    {
        private readonly ILogger _logger;
        private readonly IExcelFileSettings _excelFileSettings;
        public XlDataReader(ILogger<XlDataReader> logger, IExcelFileSettings excelFileSettings)
        {
            _logger = logger;
            _excelFileSettings = excelFileSettings;
        }
        public EveryoneRoster Process(Stream xlStream)
        {
            using (var excelWorkbook = new XLWorkbook(xlStream))
            {
                var employees = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var dateShifts = new List<ExcelCalDataRow>();
                var currentYear = DateTime.Today.Year;
                var allWs = from ws in excelWorkbook.Worksheets
                            where int.TryParse(ws.Name, out int wsName) && wsName >= currentYear
                            orderby ws.Name
                            select ws;
                var timer = new Stopwatch();
                foreach (var ws in allWs)
                {
                    timer.Start();
                    _logger.LogDebug($"enumerating rows of {ws.Name}");
                    int rows = 0;
                    HeaderMapper headerMapper = null;
                    using (var rowsEnumerator = ws.RowsUsed().GetEnumerator())
                    {
                        while (rowsEnumerator.MoveNext())
                        {
                            var dataRow = rowsEnumerator.Current;
                            if (headerMapper == null)
                            {
                                foreach (var cell in dataRow.CellsUsed().SkipWhile(c => c.Address.ColumnNumber <= _excelFileSettings.DateCol))
                                {
                                    if (cell.CachedValue.TryGetText(out var txt)
                                        && txt.IndexOf(_excelFileSettings.EmployeeNamesBeneath, StringComparison.OrdinalIgnoreCase) > -1)
                                    {
                                        var borderedCells = FindMergedBoundaries(cell) ?? FindBorders(cell);
                                        if (!borderedCells.IsBordered || !rowsEnumerator.MoveNext())
                                        {
                                            throw new Exception("Cannot Identify borders defining Employee Names");
                                        }
                                        dataRow = rowsEnumerator.Current;
                                        var cellsBetween = dataRow.Cells(borderedCells.LeftBorderedCol, borderedCells.RightBorderedCol);
                                        var specialShifts = new List<IXLCell>();
                                        foreach (var c in borderedCells.CellsRight(dataRow))
                                        {
                                            if (c.TryGetValue(out string v) 
                                                    && _excelFileSettings.SpecialShiftHeaders.Any(ss => v.Contains(ss)))
                                            {
                                                specialShifts.Add(c);
                                            }
                                        }
                                        headerMapper = new HeaderMapper(cellsBetween, specialShifts, _excelFileSettings);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                headerMapper.AddRow(dataRow);
                            }
                            ++rows;
                        }
                    }
                    timer.Stop();
                    _logger.LogDebug($"examined {rows} in {timer.ElapsedMilliseconds} ms = {timer.Elapsed.TotalMilliseconds/rows} ms/row");
                    timer.Reset();
                    if (headerMapper != null)
                    {
                        dateShifts.AddRange(headerMapper.Roster);
                        foreach (var e in headerMapper.AllEmployees)
                            employees.Add(e);
                    }
                }
                return new EveryoneRoster
                {
                    DailyRoster = dateShifts,
                    Employees = employees
                };
            }
        }

        private static BorderedRange FindMergedBoundaries(IXLCell cell)
        {
            var mergedIncludingCell = cell.Worksheet.MergedRanges
                .FirstOrDefault(c => c.RangeAddress.Contains(cell.Address));
            if (mergedIncludingCell != default)
            {
                return new BorderedRange
                {
                    LeftBorderedCol = mergedIncludingCell.FirstColumn().ColumnNumber(),
                    RightBorderedCol = mergedIncludingCell.LastColumn().ColumnNumber()
                };
            }
            return null;
        }
        private static BorderedRange FindBorders(IXLCell cell)
        {
            IXLCell leftBorderedCell;
            var rightBorderedCell = leftBorderedCell = cell;
            var returnVar = new BorderedRange();

            while (leftBorderedCell.Address.ColumnNumber >= 1
                && leftBorderedCell.Style.Border.LeftBorder == XLBorderStyleValues.None)
            {
                leftBorderedCell = leftBorderedCell.CellLeft();
            }
            if (leftBorderedCell.Style.Border.LeftBorder != XLBorderStyleValues.None)
                returnVar.LeftBorderedCol = leftBorderedCell.Address.ColumnNumber;

            var maxCols = cell.WorksheetRow().LastCellUsed().Address.ColumnNumber;
            while (rightBorderedCell.Address.ColumnNumber < maxCols
                && rightBorderedCell.Style.Border.RightBorder == XLBorderStyleValues.None)
            {
                rightBorderedCell = rightBorderedCell.CellRight();
            }
            if (rightBorderedCell.Style.Border.RightBorder != XLBorderStyleValues.None)
                returnVar.RightBorderedCol = rightBorderedCell.Address.ColumnNumber;

            return returnVar;
        }


    }
    internal class BorderedRange
    {
        public int LeftBorderedCol { get; set; } = -1;
        public int RightBorderedCol { get; set; } = -1;
        public bool IsBordered { get => (LeftBorderedCol | RightBorderedCol) != -1; }
        public IEnumerable<IXLCell> CellsRight(IXLRow row)
        {
            var lastColUsed = row.LastCellUsed().Address.ColumnNumber;
            return (!IsBordered || lastColUsed <= RightBorderedCol)
                ? Enumerable.Empty<IXLCell>()
                : row.Cells(RightBorderedCol, lastColUsed);
        }
        public IEnumerable<IXLCell> CellsLeft(IXLRow row)
        {
            return (!IsBordered)
                ? Enumerable.Empty<IXLCell>()
                : row.Cells(1, LeftBorderedCol);
        }
    }
}
