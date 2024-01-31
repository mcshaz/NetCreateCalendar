using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System;

namespace CreateCalendar.Utilities
{
    internal static class OpenXmlExtensions
    {
        internal static string ColumnReference(this Cell cell)
        {
            int indx = 1; // 1st will always be a column letter
            var address = cell.CellReference.Value;
            while (!char.IsDigit(address[indx])) { ++indx; }
            return address.Substring(0, indx);
        }

        internal static int ColumnReferenceC1(this Cell cell)
        {
            return ColNumber(cell.ColumnReference());
        }
        internal static int RowReference(this DocumentFormat.OpenXml.Spreadsheet.Cell cell)
        {
            int indx = 1;
            var address = cell.CellReference.Value;
            while (!char.IsDigit(address[indx])) { ++indx; }
            return int.Parse(address.Substring(indx));
        }
        internal static bool TryGetString(this Cell cell, string[] lookup, out string val)
        {
            if (cell.DataType != null)
            {
                if (cell.DataType == CellValues.InlineString)
                {
                    throw new NotImplementedException("Currently not set up to parse inline strings");
                }
                if (cell.DataType == CellValues.SharedString && int.TryParse(cell.CellValue?.Text, out int index))
                {
                    val = lookup[index];
                    return true;
                }
            }
            val = null;
            return false;
        }
        internal static bool TryGetDateOnly(this Cell cell, out DateOnly val)
        {
            if ((cell.DataType == null || cell.DataType.Value == CellValues.Number || cell.DataType.Value == CellValues.Date) && double.TryParse(cell.CellValue?.Text, out double days))
            {
                val = DateOnly.FromDayNumber((int)days + 693593);
                return true;
            }
            val = DateOnly.FromDayNumber(0);
            return false;
        }
        internal static T Closest<T>(this Cell cell) where T : OpenXmlElement
        {
            var p = cell.Parent;
            while (p != null && p.GetType() != typeof(T))
            {
                p = p.Parent;
            }
            return (T)p;
        }
        internal static int ColNumber(string columnName)
        {
            int sum = 0;

            for (int i = 0; i < columnName.Length; i++)
            {
                sum *= 26;
                sum += columnName[i] - 'A' + 1;
            }

            return sum;
        }
    }
}
