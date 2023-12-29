using ClosedXML.Excel;
using CreateCalendar.DataTransfer;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace CreateCalendar.ProcessXlCalendar
{
    public static class ReadXlData
    {
        public static EveryoneRoster Process(Stream fileStream)
        {
            using (var excelWorkbook = new XLWorkbook(fileStream))
            {
                var employees = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var dateShifts = new List<ExcelCalDataRow>();
                var currentYear = DateTime.Today.Year;
                var allWs = from ws in excelWorkbook.Worksheets
                            where int.TryParse(ws.Name, out int wsName) && wsName >= currentYear
                            orderby ws.Name
                            select ws;
                foreach (var ws in allWs)
                {
                    HeaderMapper headerMapper = null;
                    using (var rowsEnumerator = ws.RowsUsed().GetEnumerator())
                    {
                        while (rowsEnumerator.MoveNext())
                        {
                            var dataRow = rowsEnumerator.Current;
                            if (headerMapper == null)
                            {
                                foreach (var cell in dataRow.CellsUsed().Skip(1))
                                {
                                    if (cell.CachedValue.TryGetText(out var txt)
                                        && txt.IndexOf("Roster", StringComparison.OrdinalIgnoreCase) > -1)
                                    {
                                        var borderedCells = FindMergedBoundaries(cell) ?? FindBorders(cell);
                                        if (!borderedCells.IsBordered || !rowsEnumerator.MoveNext())
                                        {
                                            throw new Exception("Cannot Identify borders defining Employee Names");
                                        }
                                        dataRow = rowsEnumerator.Current;
                                        var cellsBetween = dataRow.Cells(borderedCells.LeftBorderedCol, borderedCells.RightBorderedCol);
                                        IEnumerable<IXLCell> specialShifts = null;
                                        foreach (var c in borderedCells.CellsRight(dataRow))
                                        {
                                            if (c.TryGetValue(out string v) && v.StartsWith("2nd"))
                                            {
                                                specialShifts = new[] { c };
                                                break;
                                            }
                                        }
                                        headerMapper = new HeaderMapper(cellsBetween, specialShifts);
                                        break;
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

            var maxRows = cell.WorksheetRow().LastCellUsed().Address.ColumnNumber;
            while (rightBorderedCell.Address.ColumnNumber < maxRows
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
    }
}
