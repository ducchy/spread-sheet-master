using UnityEditor;

namespace SpreadSheetMaster.Editor {
	public partial class SpreadSheetMasterGeneratorWindow {
		#region Methods

		/// <summary> 設定選択セクションの描画 </summary>
		private void DrawSectionSelectSettings() {
			DrawSectionHeader("設定データ選択");
			DrawSectionContent(() => {
				Undo.RecordObject(this, "Modify Settings Data Asset");
				_settings = (SpreadSheetSettings)EditorGUILayout.ObjectField(_settings, typeof(SpreadSheetSettings), false);
			});
		}

		#endregion
	}
}