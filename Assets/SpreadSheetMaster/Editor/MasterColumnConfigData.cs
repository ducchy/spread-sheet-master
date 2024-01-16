namespace SpreadSheetMaster.Editor {
	[System.Serializable]
	public class MasterColumnConfigData {
		#region Serialize Fields

		public bool _validFlag;
		public bool _exportFlag;
		public string _propertyName;
		public string _constantName;
		public DataType _type;
		public string _enumTypeName;

		#endregion

		#region Variables

		public string TypeName => _type == DataType.Enum ? _enumTypeName : _type.ToString().ToLower();
		public System.Type EnumType;

		#endregion
	}
}