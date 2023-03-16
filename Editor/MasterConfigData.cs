namespace SpreadSheetMaster.Editor
{
    [System.Serializable]
    public class MasterConfigData
    {
        public string masterName;
        public string masterDataName => masterName + "Data";
        public string spreadSheetId;
        public string sheetId;
        public MasterColumnConfigData[] columns;
        public MasterColumnConfigData _idMasterColumnConfigData;
    }
}