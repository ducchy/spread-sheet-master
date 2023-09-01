using System.Collections.Generic;

namespace SpreadSheetMaster
{
    /// <summary> インポート可能なスプレッドシートマスタ </summary>
    public interface IImportableSpreadSheetMaster
    {
        /// <summary> クラス名 </summary>
        string className { get; }

        /// <summary> インポート </summary>
        void Import(IReadOnlyList<IReadOnlyList<string>> records, ImportMasterLogBuilder importLogBuilder);
    }
}