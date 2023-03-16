namespace SpreadSheetMaster.Editor
{
    [System.Serializable]
    public class MasterColumnConfigData
    {
        public bool validFlag;
        public bool exportFlag;
        public string propertyName;
        public string constantName;
        public DataType type;
        public string typeName => type == DataType.Enum ? enumTypeName : type.ToString().ToLower();
        public System.Type enumType;
        public string enumTypeName;
    }
}