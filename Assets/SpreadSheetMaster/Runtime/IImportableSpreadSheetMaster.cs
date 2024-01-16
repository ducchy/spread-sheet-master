using System.Collections.Generic;

namespace SpreadSheetMaster {
	/// <summary> インポート可能なスプレッドシートマスタ </summary>
	public interface IImportableSpreadSheetMaster {
		#region Variables

		/// <summary> クラス名 </summary>
		string ClassName { get; }

		#endregion

		#region Methods

		/// <summary> インポート </summary>
		void Import(IReadOnlyList<IReadOnlyList<string>> records, ImportMasterLogBuilder importLogBuilder);

		#endregion
	}
}