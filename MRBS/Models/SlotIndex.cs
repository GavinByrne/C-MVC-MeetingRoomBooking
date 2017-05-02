using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRBS.Models
{
    public class SlotIndex
    {
        public SlotIndex(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public int Col { get; set; }
        public int Row { get; set; }
    }

    public static class SlotIndexExtensions
    {
        public static bool IsEmptySlotRequired(this IEnumerable<SlotIndex> skipEmptySlotIndexes, int colIndex, int rowIndex)
        {
            return skipEmptySlotIndexes.Any(l => l.Col == colIndex && l.Row == rowIndex) == false;
        }
    }
}