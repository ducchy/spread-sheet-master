using System;

namespace SpreadSheetMaster
{
    [Serializable]
    public class IgnoreColumnCondition
    {
        public IgnoreColumnConditionType type;
        public string conditionString;

        public bool IsIgnore(string columnName)
        {
            switch (type)
            {
                case IgnoreColumnConditionType.ContainString:
                    return columnName.IndexOf(conditionString, StringComparison.Ordinal) != -1;
                default:
                    return false;
            }
        }
    }

    [Serializable]
    public enum IgnoreColumnConditionType
    {
        ContainString,
    }
}