namespace SpreadSheetMaster.Editor
{
    [System.Serializable]
    public class MasterConfigData
    {
        public string masterName;
        public string masterDataName => masterName + "Data";
        public string sheetName;
        public string exportNamespaceName;
        public MasterColumnConfigData[] columns;
        public MasterColumnConfigData idMasterColumnConfigData;
        public MasterColumnConfigData maxMasterColumnConfigData;
    }
}