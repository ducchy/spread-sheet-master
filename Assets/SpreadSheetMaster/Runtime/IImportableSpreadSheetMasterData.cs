using System.Collections.Generic;

namespace SpreadSheetMaster {
	/// <summary> インポート可能なスプレッドシートマスタデータ </summary>
	public interface IImportableSpreadSheetMasterData {
		#region Methods

		/// <summary> キー取得 </summary>
		int GetKey();

		/// <summary> データ設定 </summary>
		void SetData(IReadOnlyList<string> record, ImportMasterLogBuilder importLogBuilder);

		#endregion
	}
}