using System;

namespace SpreadSheetMaster
{
    /// <summary> 列無視条件 </summary>
    [Serializable]
    public class IgnoreColumnCondition
    {
        /// <summary> 無視条件タイプ </summary>
        [Serializable]
        public enum Type
        {
            /// <summary> 特定の文字列を含む </summary>
            ContainString,
        }

        /// <summary> 無視条件タイプ </summary>
        public Type type;
        
        /// <summary> 条件文字列 </summary>
        public string conditionString;

        /// <summary> 無視条件か </summary>
        public bool IsIgnore(string columnName)
        {
            switch (type)
            {
                case Type.ContainString:
                    return columnName.IndexOf(conditionString, StringComparison.Ordinal) != -1;
                default:
                    return false;
            }
        }
    }
}