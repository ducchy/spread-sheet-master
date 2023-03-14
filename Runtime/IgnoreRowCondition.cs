using System;

namespace SpreadSheetMaster
{
    [Serializable]
    public class IgnoreRowCondition
    {
        public IgnoreRowConditionType type;
        public int conditionInt;

        public bool IsIgnore(int row)
        {
            switch (type)
            {
                case IgnoreRowConditionType.MatchLine:
                    return row == conditionInt;
                default: return false;
            }
        }
    }

    [Serializable]
    public enum IgnoreRowConditionType
    {
        MatchLine,
    }
}