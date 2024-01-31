using CreateCalendar.Utilities;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;

namespace CreateCalendar.ProcessXlCalendar
{
    internal class MergedRangeFinder
    {
        private readonly List<AddressRange> _mergedRanges;
        public MergedRangeFinder(Worksheet ws) 
        { 
            _mergedRanges = new List<AddressRange>();
            foreach (var mergeCells in ws.Elements<MergeCells>())
            {
                foreach (MergeCell mergeCell in mergeCells.OfType<MergeCell>())
                {
                    if (mergeCell.Reference != null && mergeCell.Reference.Value != null)
                    {
                        _mergedRanges.Add(new AddressRange(mergeCell.Reference.Value));
                    }
                    
                }
            }
        }
        public ColumnBoundaries? IncludesCell(Cell cell)
        {
            var foundMergedRange = _mergedRanges.FirstOrNull(mr => mr.Contains(cell));
            return foundMergedRange.HasValue
                ? (ColumnBoundaries?)new ColumnBoundaries(foundMergedRange.Value)
                : null;
        }
    }
}
