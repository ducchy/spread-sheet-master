using System;

namespace SpreadSheetMaster {
	/// <summary> 列無視条件 </summary>
	[Serializable]
	public class IgnoreColumnCondition {
		#region Constants

		/// <summary> 無視条件タイプ </summary>
		[Serializable]
		public enum Type {
			/// <summary> 特定の文字列を含む </summary>
			ContainString,
		}

		#endregion

		#region Serialize Fields

		/// <summary> 無視条件タイプ </summary>
		public Type _type;

		/// <summary> 条件文字列 </summary>
		public string _conditionString;

		#endregion

		#region Methods

		/// <summary> 無視条件か </summary>
		public bool IsIgnore(string columnName) {
			switch (_type) {
				case Type.ContainString:
					return columnName.IndexOf(_conditionString, StringComparison.Ordinal) != -1;
				default:
					return false;
			}
		}

		#endregion
	}
}