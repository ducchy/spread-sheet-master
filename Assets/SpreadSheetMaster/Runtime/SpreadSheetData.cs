using System;
using UnityEngine;

namespace SpreadSheetMaster {
	/// <summary> スプレッドシート情報 </summary>
	[Serializable]
	public class SpreadSheetData {
		#region Serialize Fields

		[SerializeField] private string _id;

		#endregion

		#region Variables

		/// <summary> ID </summary>
		public string Id => _id;

		#endregion
	}
}