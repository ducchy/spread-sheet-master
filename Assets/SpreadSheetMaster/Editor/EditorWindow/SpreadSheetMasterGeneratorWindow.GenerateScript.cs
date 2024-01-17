using System;
using UnityEditor;
using UnityEngine;

namespace SpreadSheetMaster.Editor {
	public partial class SpreadSheetMasterGeneratorWindow {
		#region Methods

		/// <summary> スクリプト生成セクションの描画 </summary>
		private void DrawSectionGenerateScript() {
			DrawSectionHeader("スクリプト生成");
			DrawSectionContent(() => {
				// バリデーション
				if (!ValidationSectionGenerateScript(out var message)) {
					DrawWarning(message);
					return;
				}

				EditorGUILayout.LabelField("生成先フォルダ", _settings.ExportScriptDirectoryPath);
				EditorGUILayout.LabelField("名前空間", _settings.ExportNamespaceName);

				if (GUILayout.Button("生成")) {
					GenerateMasterAndMasterDataScript(_editMasterConfig, true);
				}

				if (GUILayout.Button("一括生成")) {
					DownloadAndExportScriptAllAsync(_settings.SpreadSheetId, Token);
				}
			});
		}

		/// <summary> バリデーション </summary>
		private bool ValidationSectionGenerateScript(out string message) {
			if (!DownloadedContentFlag) {
				message = "シートをダウンロードしてください";
				return false;
			}

			if (_editMasterConfig._idMasterColumnConfigData == null) {
				message = "\"id\"カラムを設定してください。";
				return false;
			}

			if (_settings.ExportScriptDirectoryPath.IndexOf("Assets/", StringComparison.Ordinal) != 0) {
				message = "生成先フォルダは \"Assets/\"から始まるパスを指定してください。";
				return false;
			}

			if (StringUtility.IsExistInvalidPathChars(_settings.ExportScriptDirectoryPath)) {
				message = "生成先フォルダのパスに使用できない文字が含まれています。";
				return false;
			}

			var fileName = $"{_editMasterConfig._masterName}.cs";
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