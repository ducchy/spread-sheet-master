using System;

namespace SpreadSheetMaster
{
    /// <summary> シート情報 </summary>
    [Serializable]
    public class SheetData
    {
        /// <summary> 名前 </summary>
        public string name;

        /// <summary> ID </summary>
        public string id;

        /// <summary> マスタ名 </summary>
        public string masterName;
    }
}