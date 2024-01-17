using System;
using UnityEditor;
using UnityEngine;

namespace SpreadSheetMaster.Editor {
	public partial class SpreadSheetMasterGeneratorWindow {
		#region Methods

		/// <summary> csv生成セクションの描画 </summary>
		private void DrawSectionGenerateCsv() {
			EditorGUILayout.LabelField("csv生成", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				// バリデーション
				if (!ValidationSectionGenerateCsv(out var message)) {
					DrawWarning(message);
					return;
				}

				EditorGUILayout.LabelField("生成先フォルダ", _settings.ExportCsvDirectoryPath);

				if (GUILayout.Button("生成")) {
					GenerateMasterCsv(_editMasterConfig, true);
				}

				if (GUILayout.Button("一括生成")) {
					DownloadAndExportCsvAllAsync(_settings.SpreadSheetId, Token);
				}
			}
		}

		/// <summary> バリデーション </summary>
		private bool ValidationSectionGenerateCsv(out string message) {
			if (!DownloadedContentFlag) {
				message = "シートをダウンロードしてください";
				return false;
			}

			if (_settings.ExportCsvDirectoryPath.IndexOf("Assets/", StringComparison.Ordinal) != 0) {
				message = "生成先フォルダは \"Assets/\"から始まるパスを指定してください。";
				return false;
			}

			if (StringUtility.IsExistInvalidPathChars(_settings.ExportCsvDirectoryPath)) {
				message = "生成先フォルダのパスに使用できない文字が含まれています。";
				return false;
			}

			var fileName = $"{_editMasterConfig._masterName}.csv";
			if (StringUtility.IsExistInvalidFileNameChars(fileName)) {
				message = "ファイル名に使用できない文字が含まれています。";
				return false;
			}

			message = string.Empty;
			return true;
		}

		#endregion
	}
}