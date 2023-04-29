namespace SpreadSheetMaster
{
    using System.Collections.Generic;

    public interface IImportableSpreadSheetMasterData
    {
        int GetKey();
        void SetData(IReadOnlyList<string> record, ImportMasterLogBuilder importLogBuilder);
    }
}