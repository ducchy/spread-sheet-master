namespace SpreadSheetMaster.Editor {
	[System.Serializable]
	public class MasterConfigData {
		#region Serialize Fields

		public string masterName;
		public string sheetName;
		public string exportNamespaceName;
		public MasterColumnConfigData[] columns;
		public MasterColumnConfigData idMasterColumnConfigData;
		public MasterColumnConfigData maxMasterColumnConfigData;

		#endregion

		#region Variables

		public string masterDataName => masterName + "Data";

		#endregion
	}
}