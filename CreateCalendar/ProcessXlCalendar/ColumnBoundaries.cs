using CreateCalendar.Utilities;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CreateCalendar.ProcessXlCalendar
{
    internal readonly struct ColumnBoundaries
    {
        public ColumnBoundaries(string left, string right)
        {
            Left = left;
            Right = right;
        }
        public ColumnBoundaries(AddressRange range)
        {
            Left = range.Start.Column; 
            Right = range.End.Column;
        }
        public readonly string Left;
        public readonly string Right;
        public IEnumerable<Cell> ApplyToRow(Row row)
        {
            var left = Left;
            var right = Right; ;
            return row.ChildElements.OfType<Cell>()
                .SkipWhile(c => ColLetterComparer.Instance.Compare(c.ColumnReference(), left) < 0)
                .TakeWhile(c => ColLetterComparer.Instance.Compare(c.ColumnReference(), right) <= 0)
                .ToList();
        }
        public IEnumerable<Cell> CellsRight(Row row)
        {
            var right = Right;
            return row.ChildElements.OfType<Cell>()
                .SkipWhile(c => ColLetterComparer.Instance.Compare(c.ColumnReference(), right) <= 0);
        }
    }
}
