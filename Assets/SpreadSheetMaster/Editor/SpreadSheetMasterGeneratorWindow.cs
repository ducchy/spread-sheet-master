namespace SpreadSheetMaster.Editor
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using UnityEditor;
	using UnityEngine;

	public class SpreadSheetMasterGeneratorWindow : EditorWindow
	{
		[MenuItem("Window/Spread Sheet Master Generator")]
		static public void ShowSpreadSheetMasterWindow()
		{
			var window = GetWindow<SpreadSheetMasterGeneratorWindow>();
			window.titleContent = new GUIContent("Spread Sheet Master Generator");
			window.Focus();
		}

		[System.Serializable]
		private class MasterConfigData
		{
			[System.Serializable]
			public class Column
			{
				public bool validFlag;
				public bool exportFlag;
				public string name;
				public string constantName;
				public DataType type;
				public string typeName => type == DataType.Enum ? enumTypeName : type.ToString().ToLower();
				public System.Type enumType;
				public string enumTypeName;
			}

			public string masterName;
			public string masterDataName => masterName + "Data";
			public string spreadSheetId;
			public string sheetName;
			public Column[] columns;
		}

		private const string DEFAULT_DIRECTORY_PATH = "Assets/";

		private readonly System.Text.RegularExpressions.Regex INVALID_FILENAME_REGEX =
			new System.Text.RegularExpressions.Regex(
				"[\\x00-\\x1f<>:\"/\\\\|?*]" +
				"|^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9]|CLOCK\\$)(\\.|$)" +
				"|[\\. ]$",
				System.Text.RegularExpressions.RegexOptions.IgnoreCase);

		[SerializeField]
		private string _spreadSheetId;
		[SerializeField]
		private string _sheetName;
		[SerializeField]
		private MasterConfigData _editMasterConfig = null;
		[SerializeField]
		private string _namespaceName;
		[SerializeField]
		private string _directoryPath = DEFAULT_DIRECTORY_PATH;

		private bool _downloadingFlag;
		private string _downloadText;
		private string _downloadSheetWarning;
		private string _downloadSheetError;
		private string _generateScriptWarning;
		private Vector2 _scrollPositionCsvPreview;
		private Vector2 _scrollPositionColumns;

		private readonly CancellationTokenSource _cts = new CancellationTokenSource();
		private CancellationToken ct => _cts.Token;


		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		private void OnDestroy()
		{
			_cts.Cancel();
			_cts.Dispose();
		}


		#region draw_window

		private void OnGUI()
		{
			EditorGUILayout.Space();

			using (new EditorGUI.DisabledScope(_downloadingFlag))
			{
				// シートのダウンロード
				DrawDownloadSheet();

				EditorGUILayout.Space();

				// マスタ構成の編集
				DrawEditMasterConfig();

				EditorGUILayout.Space();

				// マスタスクリプト生成
				DrawGenerateMasterScript();
			}

			GUILayout.FlexibleSpace();
		}

		private void DrawDownloadSheet()
		{
			EditorGUILayout.LabelField("シートのダウンロード", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				Undo.RecordObject(this, "Modify SpreadSheetId or SheetName");
				_spreadSheetId = EditorGUILayout.TextField("スプレッドシートID", _spreadSheetId);
				_sheetName = EditorGUILayout.TextField("シート名", _sheetName);

				ValidationSheetData();

				if (!string.IsNullOrEmpty(_downloadSheetWarning))
					EditorGUILayout.HelpBox(_downloadSheetWarning, MessageType.Warning);

				using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(_downloadSheetWarning)))
				{
					if (GUILayout.Button("ダウンロード"))
					{
						_downloadingFlag = true;
						_downloadText = "ダウンロード中...";

						_editMasterConfig = null;
						_downloadText = string.Empty;
						_downloadSheetError = string.Empty;
						EditorGUIUtility.editingTextField = false;

						DownloadSheetAsync();
					}
				}

				if (!string.IsNullOrEmpty(_downloadSheetError))
					EditorGUILayout.HelpBox(_downloadSheetError, MessageType.Error);

				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(120)))
				{
					using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPositionCsvPreview))
					{
						_scrollPositionCsvPreview = scroll.scrollPosition;
						using (new EditorGUI.DisabledScope(true))
						{
							EditorGUILayout.TextArea(_downloadText == string.Empty ? "ダウンロードしたシートの内容が表示されます" : _downloadText, GUILayout.MinHeight(110));
						}
					}
				}

				using (new EditorGUI.DisabledScope(_downloadText == string.Empty))
				{
					if (GUILayout.Button("クリア"))
					{
						_editMasterConfig = null;
						_downloadText = string.Empty;
						_downloadSheetError = string.Empty;
						EditorGUIUtility.editingTextField = false;
					}
				}
			}
		}

		private async void DownloadSheetAsync()
		{
			SheetDownloader downloader = new SheetDownloader();
			await downloader.DownloadSheetAsync(_spreadSheetId, _sheetName, (response) =>
			{
				_downloadText = response;
				_editMasterConfig = ParseCsvToConfig(_downloadText, _spreadSheetId, _sheetName);
				_downloadingFlag = false;
			}, (error) =>
			{
				_downloadSheetError = error;
				_downloadingFlag = false;
			}, ct);
		}

		private void DrawEditMasterConfig()
		{
			EditorGUILayout.LabelField("マスタ構成の編集", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (string.IsNullOrEmpty(_downloadText))
				{
					EditorGUILayout.LabelField("シートをダウンロードしてください");
					return;
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					Undo.RecordObject(this, "Modify Config MasterName");
					EditorGUILayout.LabelField("マスタ名", GUILayout.MaxWidth(80));
					_editMasterConfig.masterName = EditorGUILayout.TextField(_editMasterConfig.masterName);
				}

				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("", EditorStyles.boldLabel, GUILayout.MaxWidth(20));
						EditorGUILayout.LabelField("使用", EditorStyles.boldLabel, GUILayout.MaxWidth(40));
						EditorGUILayout.LabelField("カラム名", EditorStyles.boldLabel, GUILayout.MaxWidth(200));
						EditorGUILayout.LabelField("データ型", EditorStyles.boldLabel, GUILayout.MaxWidth(80));
						EditorGUILayout.LabelField("Enum名", EditorStyles.boldLabel, GUILayout.MaxWidth(280));

						GUILayout.FlexibleSpace();
					}

					EditorGUILayout.Space();

					if (_editMasterConfig.columns == null || _editMasterConfig.columns.Length == 0)
					{
						EditorGUILayout.LabelField("カラムの取得に失敗");
						return;
					}

					Undo.RecordObject(this, "Modify Config Columns");
					using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPositionColumns))
					{
						_scrollPositionColumns = scroll.scrollPosition;

						for (int i = 0; i < _editMasterConfig.columns.Length; ++i)
						{
							var column = _editMasterConfig.columns[i];

							using (new EditorGUILayout.HorizontalScope())
							{
								EditorGUILayout.LabelField(column.validFlag ? "○" : "×", GUILayout.MaxWidth(20));
								column.exportFlag = EditorGUILayout.Toggle(column.exportFlag, GUILayout.MaxWidth(40));
								EditorGUILayout.LabelField(column.name, GUILayout.MaxWidth(200));
								DataType dataType = (DataType)EditorGUILayout.EnumPopup(column.type, GUILayout.MaxWidth(80));

								using (new EditorGUI.DisabledScope(column.type != DataType.Enum))
								{
									column.enumTypeName = EditorGUILayout.TextField(column.enumTypeName, GUILayout.MaxWidth(240));

									if (GUILayout.Button("適用", GUILayout.MaxWidth(40)))
									{
										System.Type type = System.Type.GetType(column.enumTypeName + ", Assembly-CSharp.dll");
										bool isEnum = type != null && type.IsEnum;
										column.validFlag = isEnum;
										column.enumType = isEnum ? type : null;
									}

								}

								GUILayout.FlexibleSpace();

								if (column.type != dataType)
								{
									column.type = dataType;
									if (dataType == DataType.Enum)
										column.validFlag = false;
									else
									{
										column.validFlag = true;
										column.enumTypeName = string.Empty;
										column.enumType = null;
									}
								}
							}
						}
					}
				}
			}
		}

		private void DrawGenerateMasterScript()
		{
			EditorGUILayout.LabelField("マスタスクリプト生成", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (string.IsNullOrEmpty(_downloadText))
				{
					EditorGUILayout.LabelField("シートをダウンロードしてください");
					return;
				}

				Undo.RecordObject(this, "Modify ExportMasterScript");
				if (GUILayout.Button("生成先フォルダ選択"))
				{
					string path = EditorUtility.OpenFolderPanel("出力先フォルダ選択", Application.dataPath, string.Empty);
					if (!string.IsNullOrEmpty(path))
						_directoryPath = GetAssetsPath(path);
				}
				_directoryPath = EditorGUILayout.TextField("生成先フォルダ", _directoryPath);
				_namespaceName = EditorGUILayout.TextField("名前空間", _namespaceName);

				ValidationGenerateScript();

				if (!string.IsNullOrEmpty(_generateScriptWarning))
					EditorGUILayout.HelpBox(_generateScriptWarning, MessageType.Warning);

				using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(_generateScriptWarning)))
				{
					if (GUILayout.Button("生成"))
						GenerateMasterAndMasterDataScript(_editMasterConfig);
				}
			}
		}

		#endregion draw_window


		private void ValidationSheetData()
		{
			_downloadSheetWarning = string.Empty;

			if (string.IsNullOrEmpty(_spreadSheetId))
				_downloadSheetWarning = "スプレッドシートID を入力してください";

			if (string.IsNullOrEmpty(_sheetName))
				_downloadSheetWarning = "シート名 を入力してください";
		}

		private MasterConfigData ParseCsvToConfig(string csv, string spreadSheetId, string sheetName)
		{
			CsvParser parser = new CsvParser();
			var records = parser.Perse(csv, excludeHeader: false);

			return CreateMasterConfigData(spreadSheetId, sheetName, records);
		}


		#region create_config_data

		private MasterConfigData CreateMasterConfigData(string spreadSheetId, string sheetName, IReadOnlyList<IReadOnlyList<string>> records)
		{
			var config = new MasterConfigData();
			config.masterName = SnakeToUpperCamel(sheetName + "Master");
			config.spreadSheetId = spreadSheetId;
			config.sheetName = sheetName;

			if (records == null || records.Count == 0)
				return config;

			var columnNames = records[0];
			var columnDatas = (records.Count >= 1) ? records[1] : null;
			int columnCount = columnNames.Count;
			if (columnCount == 0)
				return config;

			config.columns = new MasterConfigData.Column[columnCount];
			for (int i = 0; i < columnCount; i++)
			{
				string columnData = (columnDatas != null && columnDatas.Count > i) ? columnDatas[i] : string.Empty;
				config.columns[i] = CreateMasterConfigColumnData(columnNames[i], columnData);
			}
			return config;
		}

		private MasterConfigData.Column CreateMasterConfigColumnData(string name, string data)
		{
			var column = new MasterConfigData.Column();
			column.validFlag = true;
			column.exportFlag = name.IndexOf("#") == -1;
			name = name.Replace("#", "");
			column.name = SnakeToLowerCamel(name);
			column.constantName = string.Format("COLUMN_{0}", SnakeToConstant(name));
			column.type = GetDataTypeFromString(data);
			column.enumType = null;
			column.enumTypeName = string.Empty;
			return column;
		}

		private DataType GetDataTypeFromString(string dataString)
		{
			if (string.IsNullOrEmpty(dataString))
				return DataType.String;

			if (int.TryParse(dataString, out int intValue))
				return DataType.Int;
			if (float.TryParse(dataString, out float floatValue))
				return DataType.Float;
			if (bool.TryParse(dataString, out bool boolValue))
				return DataType.Bool;

			return DataType.String;
		}

		private string SnakeToUpperCamel(string snake)
		{
			if (string.IsNullOrEmpty(snake))
				return snake;

			return snake
				.Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries)
				.Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
				.Aggregate(string.Empty, (s1, s2) => s1 + s2);
		}

		private string SnakeToLowerCamel(string snake)
		{
			if (string.IsNullOrEmpty(snake))
				return snake;

			return SnakeToUpperCamel(snake)
				.Insert(0, char.ToLowerInvariant(snake[0]).ToString()).Remove(1, 1);
		}

		private string SnakeToConstant(string snake)
		{
			if (string.IsNullOrEmpty(snake))
				return snake;

			return snake.ToUpper();
		}

		#endregion create_config_data


		#region generate_master_script

		private void ValidationGenerateScript()
		{
			_generateScriptWarning = string.Empty;
			if (_directoryPath.IndexOf("Assets/") != 0)
				_generateScriptWarning = "生成先フォルダは \"Assets/\"から始まるパスを指定してください。";

			if (_directoryPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
				_generateScriptWarning = "生成先フォルダのパスに使用できない文字が含まれています。";

			string fileName = string.Format("{0}.cs", _editMasterConfig.masterName);
			if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || INVALID_FILENAME_REGEX.IsMatch(fileName))
				_generateScriptWarning = "ファイル名に使用できない文字が含まれています。";
		}

		private string GetAssetsPath(string fullPath)
		{
			int startIndex = fullPath.IndexOf("Assets/", System.StringComparison.Ordinal);
			if (startIndex == -1) startIndex = fullPath.IndexOf("Assets\\", System.StringComparison.Ordinal);
			if (startIndex == -1) return "";

			string assetPath = fullPath.Substring(startIndex);
			return assetPath;
		}

		private void GenerateMasterAndMasterDataScript(MasterConfigData configData)
		{
			CreateDirectoryIfNeeded(_directoryPath);

			GenerateMasterScript(_directoryPath, configData, _namespaceName);
			GenerateMasterDataScript(_directoryPath, configData, _namespaceName);

			AssetDatabase.Refresh();
		}

		private void CreateDirectoryIfNeeded(string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);
		}

		private void GenerateMasterScript(string directoryPath, MasterConfigData configData, string namespaceName)
		{
			string exportPath = string.Format("{0}/{1}.cs", directoryPath, configData.masterName);
			File.WriteAllText(exportPath, CreateMasterScriptContent(configData, namespaceName));
		}

		private void GenerateMasterDataScript(string directoryPath, MasterConfigData configData, string namespaceName)
		{
			string exportPath = string.Format("{0}/{1}.cs", directoryPath, configData.masterDataName);
			File.WriteAllText(exportPath, CreateMasterDataScriptContent(configData, namespaceName));
		}

		private string CreateMasterScriptContent(MasterConfigData configData, string namespaceName)
		{
			StringBuilder sb = new StringBuilder();
			int tabCount = 0;

			bool namespaceExistFlag = !string.IsNullOrEmpty(namespaceName);

			AppendTab(sb, tabCount).Append("using SpreadSheetMaster;").AppendLine();
			sb.AppendLine();

			if (namespaceExistFlag)
			{
				AppendTab(sb, tabCount).AppendFormat("namespace {0}", namespaceName).AppendLine();
				AppendTab(sb, tabCount++).Append("{").AppendLine();
			}

			AppendTab(sb, tabCount).AppendFormat("public partial class {0} : ImportableSpreadSheetMasterBase<{1}>", configData.masterName, configData.masterDataName).AppendLine();
			AppendTab(sb, tabCount++).Append("{").AppendLine();
			AppendTab(sb, tabCount).AppendFormat("public override string spreadSheetId => \"{0}\";", configData.spreadSheetId).AppendLine();
			AppendTab(sb, tabCount).AppendFormat("public override string sheetName => \"{0}\";", configData.sheetName).AppendLine();
			AppendTab(sb, --tabCount).Append("}").AppendLine();

			if (namespaceExistFlag)
				AppendTab(sb, --tabCount).Append("}").AppendLine();

			return sb.ToString();
		}

		private string CreateMasterDataScriptContent(MasterConfigData configData, string namespaceName)
		{
			StringBuilder sb = new StringBuilder();
			int tabCount = 0;

			bool namespaceExistFlag = !string.IsNullOrEmpty(namespaceName);

			var indexColumnList = new List<System.Tuple<int, MasterConfigData.Column>>();
			for (int i = 0; i < configData.columns.Length; i++)
			{
				var column = configData.columns[i];
				if (column.exportFlag && column.validFlag)
					indexColumnList.Add(System.Tuple.Create(i, column));
			}

			AppendTab(sb, tabCount).Append("using SpreadSheetMaster;").AppendLine();
			AppendTab(sb, tabCount).Append("using System.Collections.Generic;").AppendLine();
			sb.AppendLine();

			if (namespaceExistFlag)
			{
				AppendTab(sb, tabCount).AppendFormat("namespace {0}", namespaceName).AppendLine();
				AppendTab(sb, tabCount++).Append("{").AppendLine();
			}

			AppendTab(sb, tabCount).AppendFormat("public partial class {0} : ImportableSpreadSheetMasterDataBase", configData.masterDataName).AppendLine();
			AppendTab(sb, tabCount++).Append("{").AppendLine();

			foreach (var tuple in indexColumnList)
				AppendTab(sb, tabCount).AppendFormat("private const int {0} = {1};", tuple.Item2.constantName, tuple.Item1).AppendLine();

			sb.AppendLine();

			foreach (var tuple in indexColumnList)
				AppendTab(sb, tabCount).AppendFormat("public {0} {1} ", tuple.Item2.typeName, tuple.Item2.name).Append("{ get; private set; }").AppendLine();

			sb.AppendLine();

			AppendTab(sb, tabCount).Append("public override int GetId()").AppendLine();
			AppendTab(sb, tabCount++).Append("{").AppendLine();
			AppendTab(sb, tabCount).Append("return id;").AppendLine();
			AppendTab(sb, --tabCount).Append("}").AppendLine();

			AppendTab(sb, tabCount).Append("public override void SetData(IReadOnlyList<string> record)").AppendLine();
			AppendTab(sb, tabCount++).Append("{").AppendLine();

			foreach (var tuple in indexColumnList)
				AppendTab(sb, tabCount).AppendFormat("{0} = Get{1}(record, {2});", tuple.Item2.name, tuple.Item2.type.ToString(), tuple.Item2.constantName).AppendLine();

			AppendTab(sb, --tabCount).Append("}").AppendLine();

			AppendTab(sb, --tabCount).Append("}").AppendLine();

			if (namespaceExistFlag)
				AppendTab(sb, --tabCount).Append("}").AppendLine();

			return sb.ToString();
		}

		private StringBuilder AppendTab(StringBuilder sb, int tabCount)
		{
			return sb.Append('\t', tabCount);
		}

		#endregion export_master_script

	}   // class SpreadSheetMasterWindow
}   // namespace SpreadSheetMaster.Editor
