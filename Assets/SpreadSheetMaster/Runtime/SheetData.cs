using System;
using UnityEngine;

namespace SpreadSheetMaster {
	/// <summary> シート情報 </summary>
	[Serializable]
	public class SheetData {
		#region Serialize Fields

		[SerializeField] private string _name;
		[SerializeField] private string _id;
		[SerializeField] private string _masterName;

		#endregion

		#region Variables

		/// <summary> 名前 </summary>
		public string Name => _name;

		/// <summary> ID </summary>
		public string Id => _id;

		/// <summary> マスタ名 </summary>
		public string MasterName => _masterName;

		#endregion
	}
}