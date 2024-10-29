using AngleSharp.Common;
using CreateCalendar.Utilities;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;

namespace CreateCalendar.ProcessXlCalendar
{
    internal class BorderFinder
    {
        private readonly HashSet<uint> _leftBorderedStyles;
        private readonly HashSet<uint> _rightBorderedStyles;
        public BorderFinder(WorkbookPart workbookPart) 
        {
            uint i = 0U;
            var leftBorderedIndices = new HashSet<uint>();
            var rightBorderedIndices = new HashSet<uint>();
            foreach (var b in workbookPart.WorkbookStylesPart.Stylesheet.Borders.ChildElements.OfType<Border>())
            {
                if ((b.LeftBorder?.Style ?? BorderStyleValues.None) != BorderStyleValues.None)
                {
                    leftBorderedIndices.Add(i);
                }
                if ((b.RightBorder?.Style ?? BorderStyleValues.None) != BorderStyleValues.None)
                {
                    rightBorderedIndices.Add(i);
                }
                ++i;
            }
            _leftBorderedStyles = [];
            _rightBorderedStyles = [];
            i = 0U;
            foreach (var cf in workbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ChildElements.OfType<CellFormat>())
            {

                if (leftBorderedIndices.Contains(cf.BorderId))
                {
                    _leftBorderedStyles.Add(i);
                }
                if (rightBorderedIndices.Contains(cf.BorderId))
                {
                    _rightBorderedStyles.Add(i);
                }
                ++i;
            }
        }
        public bool IsLeftBordered(Cell cell)
        {
            return cell.StyleIndex != null
                && _leftBorderedStyles.Contains(cell.StyleIndex.Value);
        }

        public bool IsRightBordered(Cell cell)
        {
            return cell.StyleIndex != null
                && _rightBorderedStyles.Contains(cell.StyleIndex.Value);
        }

        public ColumnBoundaries? FindBorders(Cell cell)
        {
            var row = (Row)cell.Parent;
            var cells = row.ChildElements;
            var i = cells.IndexOf(cell);
            int leftBorderIndex = i;
            while (leftBorderIndex >= 0 && !IsLeftBordered((Cell)cells[leftBorderIndex])) { --leftBorderIndex; }
            int rightBorderIndex = i;
            while (rightBorderIndex < cells.Count && !IsRightBordered((Cell)cells[rightBorderIndex])) { ++rightBorderIndex; }
            if (leftBorderIndex >= 0 && rightBorderIndex < cells.Count)
            {
                return new ColumnBoundaries(((Cell)cells[leftBorderIndex]).ColumnReference(),
                    ((Cell)cells[rightBorderIndex]).ColumnReference());
            }
            return null;
        }
    }
}
