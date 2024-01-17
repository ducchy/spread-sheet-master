using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SpreadSheetMaster.Editor {
	/// <summary> スプレッドシートマスタ生成ウィンドウ </summary>
	public partial class SpreadSheetMasterGeneratorWindow : EditorWindow {
		#region Serialize Fields

		[SerializeField] private SpreadSheetSettings _settings;
		[SerializeField] private int _sheetIndex;
		[SerializeField] private MasterConfigData _editMasterConfig;

		#endregion

		#region Variables

		private readonly CancellationTokenSource _cts = new();

		private bool DownloadedContentFlag => !string.IsNullOrEmpty(_downloadContent);
		private CancellationToken Token => _cts.Token;

		private SheetData _sheetData;

		private bool _downloadingFlag;
		private bool _batchDownloadingFlag;
		private string _downloadContent;
		private string _downloadSheetError;

		#endregion

		#region Unity Event Functions

		private void OnDestroy() {
			_cts.Cancel();
			_cts.Dispose();
		}

		private void OnGUI() {
			EditorGUILayout.Space();

			using (new EditorGUI.DisabledScope(_downloadingFlag || _batchDownloadingFlag)) {
				// 設定データ選択
				DrawSectionSelectSettings();

				EditorGUILayout.Space();

				// ルート情報読み込み
				DrawDownloadRootInfo();

				EditorGUILayout.Space();

				// シート読み込み
				DrawDownloadSheet();

				EditorGUILayout.Space();

				// マスタ構成の編集
				DrawSectionEditMasterConfig();

				EditorGUILayout.Space();

				// マスタスクリプト生成
				DrawSectionGenerateScript();

				EditorGUILayout.Space();

				// csv生成
				DrawSectionGenerateCsv();
			}

			GUILayout.FlexibleSpace();
		}

		#endregion

		#region Methods

		/// <summary> ウィンドウを開く </summary>
		[MenuItem("Window/Spread Sheet Master Generator")]
		public static void ShowSpreadSheetMasterWindow() {
			var window = GetWindow<SpreadSheetMasterGeneratorWindow>();
			window.titleContent = new GUIContent("Spread Sheet Master Generator");
			window.Focus();
		}

		/// <summary> シートのダウンロード </summary>
		private void DownloadSheet() {
			_downloadingFlag = true;
			_editMasterConfig = null;
			_downloadContent = string.Empty;
			_downloadSheetError = string.Empty;
			EditorGUIUtility.editingTextField = false;

			DownloadSheetAsync(Token);
		}

		/// <summary> シートのURLを開く </summary>
		private void OpenSheetUrl() {
			var urlBuilder = new SheetUrlBuilder();
			var url = urlBuilder.BuildEditUrl(_settings.SpreadSheetId, _sheetData.Id);
			Application.OpenURL(url);
		}

		/// <summary> 警告描画 </summary>
		private void DrawWarning(string message) {
			EditorGUILayout.HelpBox(message, MessageType.Warning);
		}

		/// <summary> セクションヘッダー描画 </summary>
		private void DrawSectionHeader(string text) {
			EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
		}

		/// <summary> セクション内容描画 </summary>
		private void DrawSectionContent(Action onDraw) {
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
				onDraw?.Invoke();
			}
		}

		private async void DownloadSheetAsync(CancellationToken token) {
			await DownloadSheetAsyncInternal(_settings.SpreadSheetId, _sheetData, token);
		}

		private async Task DownloadSheetAsyncInternal(string spreadSheetId, SheetData sheetData, CancellationToken token) {
			var downloader = new SheetDownloader();
			var downloadSheetAsync = downloader.DownloadSheetAsync(spreadSheetId, sheetData.Id, token);

			while (!downloadSheetAsync.IsDone) {
				await Task.Yield();
			}

			if (downloadSheetAsync.Exception != null) {
				OnDownloadError(downloadSheetAsync.Exception.Message);
				return;
			}

			OnDownloadSuccess(downloadSheetAsync.Result, sheetData);
		}

		private void OnDownloadSuccess(string response, SheetData sheetData) {
			_downloadContent = response;
			_editMasterConfig = ParseCsvToConfig(_downloadContent, sheetData.Name, sheetData.MasterName);
			_downloadingFlag = false;
		}

		private void OnDownloadError(string error) {
			_downloadSheetError = error;
			_downloadingFlag = false;
		}

		private MasterConfigData ParseCsvToConfig(
			string csv,
			string sheetName,
			string masterName) {
			ICsvParser parser = new CsvParser(_settings.IgnoreRowConditions);
			var records = parser.Parse(csv, false);

			return CreateMasterConfigData(sheetName, masterName, records);
		}

		private MasterConfigData CreateMasterConfigData(
			string sheetName,
			string masterName,
			IReadOnlyList<IReadOnlyList<string>> records) {
			var config = new MasterConfigData {
				_masterName = StringUtility.SnakeToUpperCamel(masterName + "Master"),
				_sheetName = sheetName,
				_exportNamespaceName = _settings.ExportNamespaceName,
			};

			if (records == null || records.Count == 0) {
				return config;
			}

			var columnNameList = records[0];
			var columnDataList = records.Count > 1 ? records[1] : null;
			var columnCount = columnNameList.Count;
			if (columnCount == 0) {
				return config;
			}

			config._columns = new MasterColumnConfigData[columnCount];
			for (var i = 0; i < columnCount; i++) {
				var columnData = columnDataList != null && columnDataList.Count > i
					? columnDataList[i]
					: string.Empty;
				var columnName = columnNameList[i];
				var column = CreateMasterConfigColumnData(columnName, columnData);
				if (columnName == "id") {
					config._idMasterColumnConfigData = column;
				}

				config._columns[i] = column;
			}

			config._maxMasterColumnConfigData =
				CreateMasterConfigColumnData("max", columnCount.ToString());

			return config;
		}

		private MasterColumnConfigData CreateMasterConfigColumnData(string columnName, string data) {
			var exportFlag = IsExportColumn(columnName);
			if (!exportFlag) {
				return new MasterColumnConfigData {
					_exportFlag = false,
				};
			}

			var column = new MasterColumnConfigData {
				_validFlag = true,
				_exportFlag = true,
				_propertyName = StringUtility.Convert(columnName,
					_settings.ColumnNameNamingConvention,
					_settings.PropertyNamingConvention),
				_constantName = StringUtility.Convert("column_" + columnName,
					_settings.ColumnNameNamingConvention,
					_settings.ConstantNamingConvention),
				_type = GetDataTypeFromString(data),
				EnumType = null,
				_enumTypeName = string.Empty,
			};
			return column;
		}

		private bool IsExportColumn(string columnName) {
			if (string.IsNullOrWhiteSpace(columnName)) {
				return false;
			}

			return _settings.IgnoreColumnConditions.All(ignoreColumnCondition =>
				!ignoreColumnCondition.IsIgnore(columnName));
		}

		private DataType GetDataTypeFromString(string dataString) {
			if (string.IsNullOrEmpty(dataString)) {
				return DataType.String;
			}

			if (int.TryParse(dataString, out _)) {
				return DataType.Int;
			}

			if (float.TryParse(dataString, out _)) {
				return DataType.Float;
			}

			if (bool.TryParse(dataString, out _)) {
				return DataType.Bool;
			}

			return DataType.String;
		}

		private Type FindEnumType(string enumTypeName) {
			foreach (var namespaceName in _settings.FindNamespaceNameList) {
				foreach (var assemblyName in _settings.FindAssemblyNameList) {
					var type = Type.GetType($"{namespaceName}.{enumTypeName}, {assemblyName}");
					if (type is { IsEnum: true, }) {
						return type;
					}
				}
			}

			return null;
		}

		private void GenerateMasterAndMasterDataScript(MasterConfigData configData,
			bool withRefresh) {
			var exportScriptDirectoryPath = _settings.ExportScriptDirectoryPath;
			CreateDirectoryIfNeeded(exportScriptDirectoryPath);

			GenerateMasterScript(exportScriptDirectoryPath, configData);
			GenerateMasterDataScript(exportScriptDirectoryPath, configData);

			if (withRefresh) {
				AssetDatabase.Refresh();
			}
		}

		private void CreateDirectoryIfNeeded(string directoryPath) {
			if (!Directory.Exists(directoryPath)) {
				Directory.CreateDirectory(directoryPath);
			}
		}

		private void GenerateMasterScript(string directoryPath, MasterConfigData configData) {
			var exportPath = $"{directoryPath}/{configData._masterName}.cs";
			var builder = new MasterScriptContentBuilder();
			var contents = builder.Build(configData);
			File.WriteAllText(exportPath, contents);
		}

		private void GenerateMasterDataScript(string directoryPath, MasterConfigData configData) {
			var exportPath = $"{directoryPath}/{configData.MasterDataName}.cs";
			var builder = new MasterDataScriptContentBuilder();
			var contents = builder.Build(configData);
			File.WriteAllText(exportPath, contents);
		}

		private async void DownloadAndExportScriptAllAsync(string spreadSheetId, CancellationToken token) {
			_batchDownloadingFlag = true;

			var sheetDataList = _settings.SheetDataArray;
			foreach (var sheetData in sheetDataList) {
				await DownloadSheetAsyncInternal(spreadSheetId, sheetData, token);
				GenerateMasterAndMasterDataScript(_editMasterConfig, false);
			}

			AssetDatabase.Refresh();

			_batchDownloadingFlag = false;
		}

		private void GenerateMasterCsv(MasterConfigData configData, bool withRefresh) {
			CreateDirectoryIfNeeded(_settings.ExportCsvDirectoryPath);

			var exportPath = $"{_settings.ExportCsvDirectoryPath}/{configData._masterName}.csv";
			File.WriteAllText(exportPath, _downloadContent);

			if (withRefresh) {
				AssetDatabase.Refresh();
			}
		}

		private async void DownloadAndExportCsvAllAsync(string spreadSheetId, CancellationToken token) {
			_batchDownloadingFlag = true;

			var sheetDataList = _settings.SheetDataArray;
			foreach (var sheetData in sheetDataList) {
				await DownloadSheetAsyncInternal(spreadSheetId, sheetData, token);
				GenerateMasterCsv(_editMasterConfig, false);
			}

			AssetDatabase.Refresh();

			_batchDownloadingFlag = false;
		}

		#endregion
	} // class SpreadSheetMasterWindow
} // namespace SpreadSheetMaster.Editor