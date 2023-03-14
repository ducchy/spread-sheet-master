namespace SpreadSheetMaster
{
    using System.Collections.Generic;

    public interface IImportableSpreadSheetMasterData
    {
        int GetId();
        void SetData(IReadOnlyList<string> record);
    }
}