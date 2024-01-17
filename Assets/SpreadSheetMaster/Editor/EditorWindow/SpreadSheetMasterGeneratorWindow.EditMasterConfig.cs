using UnityEditor;
using UnityEngine;

namespace SpreadSheetMaster.Editor {
	public partial class SpreadSheetMasterGeneratorWindow {
		#region Variables

		private Vector2 _scrollPositionEditMasterConfig;

		#endregion

		#region Methods

		/// <summary> マスタ構成編集セクションの描画 </summary>
		private void DrawSectionEditMasterConfig() {
			DrawSectionHeader("マスタ構成の編集");
			DrawSectionContent(() => {
				// バリデーション
				if (!ValidationSectionEditMasterConfig(out var message)) {
					DrawWarning(message);
					return;
				}

				// マスタ名
				DrawEditMasterConfigMasterName();

				// リスト
				DrawEditMasterConfigList();
			});
		}

		/// <summary> バリデーション </summary>
		private bool ValidationSectionEditMasterConfig(out string message) {
			if (!DownloadedContentFlag) {
				message = "シートをダウンロードしてください";
				return false;
			}

			message = string.Empty;
			return true;
		}

		/// <summary> マスタ名描画 </summary>
		private void DrawEditMasterConfigMasterName() {
			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.LabelField("マスタ名", GUILayout.MaxWidth(80));
				EditorGUILayout.TextField(_editMasterConfig._masterName);
			}
		}

		/// <summary> リスト描画 </summary>
		private void DrawEditMasterConfigList() {
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				// ヘッダ
				DrawEditMasterConfigListHeader();

				EditorGUILayout.Space();

				if (_editMasterConfig._columns == null || _editMasterConfig._columns.Length == 0) {
					EditorGUILayout.LabelField("カラムの取得に失敗");
					return;
				}

				// 各カラム
				Undo.RecordObject(this, "Modify Config Columns");
				using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPositionEditMasterConfig)) {
					_scrollPositionEditMasterConfig = scroll.scrollPosition;

					foreach (var column in _editMasterConfig._columns) {
						DrawEditMasterConfigColumn(column);
						GUILayout.FlexibleSpace();
					}
				}
			}
		}

		/// <summary> リストヘッダ描画 </summary>
		private void DrawEditMasterConfigListHeader() {
			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.LabelField("", EditorStyles.boldLabel, GUILayout.MaxWidth(20));
				EditorGUILayout.LabelField("使用", EditorStyles.boldLabel, GUILayout.MaxWidth(40));
				EditorGUILayout.LabelField("カラム名", EditorStyles.boldLabel, GUILayout.MaxWidth(200));
				EditorGUILayout.LabelField("データ型", EditorStyles.boldLabel, GUILayout.MaxWidth(80));
				EditorGUILayout.LabelField("Enum名", EditorStyles.boldLabel, GUILayout.MaxWidth(280));
				GUILayout.FlexibleSpace();
			}
		}

		/// <summary> リストカラム描画 </summary>
		private void DrawEditMasterConfigColumn(MasterColumnConfigData column) {
			if (string.IsNullOrWhiteSpace(column._propertyName)) {
				return;
			}

			using (new EditorGUILayout.HorizontalScope()) {
				EditorGUILayout.LabelField(column._validFlag ? "○" : "×", GUILayout.MaxWidth(20));
				column._exportFlag = EditorGUILayout.Toggle(column._exportFlag, GUILayout.MaxWidth(40));
				EditorGUILayout.LabelField(column._propertyName, GUILayout.MaxWidth(200));
				var dataType = (DataType)EditorGUILayout.EnumPopup(column._type, GUILayout.MaxWidth(80));

				using (new EditorGUI.DisabledScope(column._type != DataType.Enum)) {
					column._enumTypeName = EditorGUILayout.TextField(column._enumTypeName, GUILayout.MaxWidth(240));

					if (GUILayout.Button("適用", GUILayout.MaxWidth(40))) {
						var type = FindEnumType(column._enumTypeName);
						column._validFlag = type != null;
						column.EnumType = type;
						column._enumTypeName = type?.Name;
					}
				}

				if (column._type == dataType) {
					return;
				}

				column._type = dataType;
				if (dataType == DataType.Enum) {
					column._validFlag = false;
				} else {
					column._validFlag = true;
					column.EnumType = null;
					column._enumTypeName = string.Empty;
				}
			}
		}

		#endregion
	}
}