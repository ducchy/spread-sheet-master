using System;
using System.Collections.Generic;

namespace SpreadSheetMaster
{
    [Serializable]
    public class IgnoreRowCondition
    {
        public IgnoreRowConditionType type;
        public int conditionInt;
        public string conditionString;

        public bool IsIgnore(int row, List<string> columns)
        {
            switch (type)
            {
                case IgnoreRowConditionType.MatchLine:
                    return row == conditionInt;
                case IgnoreRowConditionType.MatchStringInFirstColumn:
                    return columns != null && columns.Count >= 1 && columns[0] == conditionString;
                default:
                    return false;
            }
        }
    }

    [Serializable]
    public enum IgnoreRowConditionType
    {
        MatchLine,
        MatchStringInFirstColumn,
    }
}