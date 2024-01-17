using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpreadSheetMaster.Editor {
	public partial class SpreadSheetMasterGeneratorWindow {
		#region Variables

		private Vector2 _scrollPositionDownloadSheet;

		#endregion

		#region Methods

		private void DrawDownloadSheet() {
			DrawSectionHeader("シート読み込み");
			DrawSectionContent(() => {
				// バリデーション
				if (!ValidationSectionDownloadSheet(out var message)) {
					DrawWarning(message);
					return;
				}

				Undo.RecordObject(this, "Modify SpreadSheetId or SheetName");

				// シート選択プルダウン
				var sheetIndexOptions = _settings.SheetDataArray.Select(sd => sd.Name).ToArray();
				_sheetIndex = EditorGUILayout.Popup("シート", _sheetIndex, sheetIndexOptions);
				_sheetData = _settings.GetSheetData(_sheetIndex);

				if (GUILayout.Button("ダウンロード")) {
					DownloadSheet();
				}

				if (GUILayout.Button("ブラウザで開く")) {
					OpenSheetUrl();
				}

				if (!string.IsNullOrEmpty(_downloadSheetError)) {
					EditorGUILayout.HelpBox(_downloadSheetError, MessageType.Error);
				}

				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(120))) {
					using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPositionDownloadSheet)) {
						_scrollPositionDownloadSheet = scroll.scrollPosition;
						using (new EditorGUI.DisabledScope(true)) {
							var text = _downloadingFlag
								? "ダウンロード中..."
								: DownloadedContentFlag
									? _downloadContent
									: "ダウンロードしたシートの内容が表示されます";
							EditorGUILayout.TextArea(text, GUILayout.MinHeight(110));
						}
					}
				}
			});
		}

		/// <summary> バリデーション </summary>
		private bool ValidationSectionDownloadSheet(out string message) {
			if (_settings == null) {
				message = "設定データを選択してください";
				return false;
			}

			if (string.IsNullOrEmpty(_settings.SpreadSheetId)) {
				message = "スプレッドシートIDが未設定";
				return false;
			}

			if (_sheetData == null) {
				message = "シート情報が未設定";
				return false;
			}

			if (string.IsNullOrEmpty(_sheetData.Id)) {
				message = "シートIDが未設定";
				return false;
			}

			if (string.IsNullOrEmpty(_sheetData.Name)) {
				message = "シート名が未設定";
				return false;
			}

			if (string.IsNullOrEmpty(_sheetData.MasterName)) {
				message = "マスタ名が未設定";
				return false;
			}

			message = string.Empty;
			return true;
		}

		#endregion
	}
}