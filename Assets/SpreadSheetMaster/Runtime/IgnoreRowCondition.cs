using System;
using System.Collections.Generic;

namespace SpreadSheetMaster
{
    /// <summary> 行無視条件 </summary>
    [Serializable]
    public class IgnoreRowCondition
    {
        /// <summary> 無視条件タイプ </summary>
        [Serializable]
        public enum Type
        {
            /// <summary> 特定の行 </summary>
            MatchLine,

            /// <summary> 先頭カラムが特定の文字列と一致 </summary>
            MatchStringInFirstColumn,
        }

        /// <summary> 無視条件タイプ </summary>
        public Type type;

        /// <summary> 条件数値 </summary>
        public int conditionInt;

        /// <summary> 条件文字列 </summary>
        public string conditionString;

        /// <summary> 無視条件か </summary>
        public bool IsIgnore(int row, List<string> columns)
        {
            switch (type)
            {
                case Type.MatchLine:
                    return row == conditionInt;
                case Type.MatchStringInFirstColumn:
                    return columns != null && columns.Count >= 1 && columns[0] == conditionString;
                default:
                    return false;
            }
        }
    }
}