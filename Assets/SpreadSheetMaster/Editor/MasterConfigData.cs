namespace SpreadSheetMaster.Editor {
	[System.Serializable]
	public class MasterConfigData {
		#region Serialize Fields

		public string _masterName;
		public string _sheetName;
		public string _exportNamespaceName;
		public MasterColumnConfigData[] _columns;
		public MasterColumnConfigData _idMasterColumnConfigData;
		public MasterColumnConfigData _maxMasterColumnConfigData;

		#endregion

		#region Variables

		public string MasterDataName => _masterName + "Data";

		#endregion
	}
}