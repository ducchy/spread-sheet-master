using System;
using System.Threading.Tasks;

namespace SpreadSheetMaster.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

        private const string DEFAULT_DIRECTORY_PATH = "Assets/";

        [SerializeField] private SpreadSheetSetting _setting;
        [SerializeField] private int _spreadSheetIndex = 0;
        [SerializeField] private int _sheetIndex = 0;
        [SerializeField] private string _spreadSheetId;
        [SerializeField] private string _sheetId;
        [SerializeField] private string _masterName;
        [SerializeField] private MasterConfigData _editMasterConfig = null;
        [SerializeField] private string _namespaceName;
        [SerializeField] private string _directoryPath = DEFAULT_DIRECTORY_PATH;

        private bool _downloadingFlag;
        private string _downloadText;
        private string _downloadSheetWarning;
        private string _downloadSheetError;
        private string _generateScriptWarning;
        private Vector2 _scrollPositionCsvPreview;
        private Vector2 _scrollPositionColumns;

        private readonly SheetUrlBuilder _sheetUrlBuilder = new SheetUrlBuilder();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private CancellationToken _token => _cts.Token;

        private string ExportDirectoryPath => _setting != null ? _setting.exportScriptDirectoryPath : _directoryPath;
        private string NamespaceName => _setting != null ? _setting.namespaceName : _namespaceName;

        private SpreadSheetData SpreadSheetData => (_setting != null && 0 <= _spreadSheetIndex &&
                                                    _spreadSheetIndex < _setting.spreadSheetDataArray.Length)
            ? _setting.spreadSheetDataArray[_spreadSheetIndex]
            : null;

        private string SpreadSheetId
        {
            get
            {
                var ssd = SpreadSheetData;
                return ssd != null ? ssd.id : _spreadSheetId;
            }
        }

        private SheetData SheetData => (_setting != null && 0 <= _sheetIndex &&
                                        _sheetIndex < _setting.sheetDataArray.Length)
            ? _setting.sheetDataArray[_sheetIndex]
            : null;

        private string SheetId
        {
            get
            {
                var sd = SheetData;
                return sd != null ? sd.id : _spreadSheetId;
            }
        }

        private string SheetMasterName
        {
            get
            {
                var sd = SheetData;
                return sd != null ? sd.masterName : _masterName;
            }
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
                // インポート設定
                DrawImportSetting();

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

        private void DrawImportSetting()
        {
            EditorGUILayout.LabelField("シート設定", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Undo.RecordObject(this, "Modify ImportSetting");
                _setting = (SpreadSheetSetting)EditorGUILayout.ObjectField(_setting, typeof(SpreadSheetSetting), false);
            }
        }

        private void DrawDownloadSheet()
        {
            EditorGUILayout.LabelField("シートのダウンロード", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Undo.RecordObject(this, "Modify SpreadSheetId or SheetName");

                if (_setting != null)
                {
                    _spreadSheetIndex = EditorGUILayout.Popup("スプレッドシート", _spreadSheetIndex,
                        _setting.spreadSheetDataArray.Select(ssd => ssd.name).ToArray());
                    _sheetIndex = EditorGUILayout.Popup("シート", _sheetIndex,
                        _setting.sheetDataArray.Select(sd => sd.name).ToArray());
                }

                using (new EditorGUI.DisabledScope(_setting != null))
                {
                    _spreadSheetId = EditorGUILayout.TextField("スプレッドシートID", SpreadSheetId);
                    _sheetId = EditorGUILayout.TextField("シートID", SheetId);
                    _masterName = EditorGUILayout.TextField("マスタ名", SheetMasterName);
                }

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

                        DownloadSheetAsync(_token);
                    }

                    if (GUILayout.Button("ブラウザで開く"))
                        Application.OpenURL(_sheetUrlBuilder.BuildEditUrl(SpreadSheetId, SheetId));
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
                            EditorGUILayout.TextArea(
                                _downloadText == string.Empty ? "ダウンロードしたシートの内容が表示されます" : _downloadText,
                                GUILayout.MinHeight(110));
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

        private async void DownloadSheetAsync(CancellationToken token)
        {
            await DownloadSheetAsync(SpreadSheetId, SheetId, SheetMasterName, token);
        }

        private async Task DownloadSheetAsync(string spreadSheetId, string sheetId, string sheetMasterName,
            CancellationToken token)
        {
            var downloader = new SheetDownloader();
            await downloader.DownloadSheetAsync(spreadSheetId, sheetId, (response) =>
            {
                _downloadText = response;
                _editMasterConfig = ParseCsvToConfig(_downloadText, spreadSheetId, sheetId, sheetMasterName);
                _downloadingFlag = false;
            }, (error) =>
            {
                _downloadSheetError = error;
                _downloadingFlag = false;
            }, token);
        }

        private async void DownloadAndExportSheetAllAsync(string spreadSheetId, CancellationToken token)
        {
            var sheetDataList = _setting.sheetDataArray;
            foreach (var sheetData in sheetDataList)
            {
                await DownloadSheetAsync(spreadSheetId, sheetData.id, sheetData.masterName, token);

                ValidationGenerateScript();
                if (!string.IsNullOrEmpty(_generateScriptWarning))
                    continue;

                GenerateMasterAndMasterDataScript(_editMasterConfig, false);
            }

            AssetDatabase.Refresh();
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

                        for (var i = 0; i < _editMasterConfig.columns.Length; ++i)
                        {
                            var column = _editMasterConfig.columns[i];

                            if (string.IsNullOrWhiteSpace(column.propertyName))
                                continue;

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField(column.validFlag ? "○" : "×", GUILayout.MaxWidth(20));
                                column.exportFlag = EditorGUILayout.Toggle(column.exportFlag, GUILayout.MaxWidth(40));
                                EditorGUILayout.LabelField(column.propertyName, GUILayout.MaxWidth(200));
                                DataType dataType =
                                    (DataType)EditorGUILayout.EnumPopup(column.type, GUILayout.MaxWidth(80));

                                using (new EditorGUI.DisabledScope(column.type != DataType.Enum))
                                {
                                    column.enumTypeName =
                                        EditorGUILayout.TextField(column.enumTypeName, GUILayout.MaxWidth(240));

                                    if (GUILayout.Button("適用", GUILayout.MaxWidth(40)))
                                    {
                                        var type =
                                            Type.GetType(column.enumTypeName + ", Assembly-CSharp.dll");
                                        var isEnum = type != null && type.IsEnum;
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

                using (new EditorGUI.DisabledScope(_setting != null))
                {
                    Undo.RecordObject(this, "Modify ExportMasterScript");
                    if (GUILayout.Button("生成先フォルダ選択"))
                    {
                        var path = EditorUtility.OpenFolderPanel("出力先フォルダ選択", Application.dataPath, string.Empty);
                        if (!string.IsNullOrEmpty(path))
                            _directoryPath = GetAssetsPath(path);
                    }

                    _directoryPath = EditorGUILayout.TextField("生成先フォルダ", ExportDirectoryPath);
                    _namespaceName = EditorGUILayout.TextField("名前空間", NamespaceName);
                }

                ValidationGenerateScript();

                if (!string.IsNullOrEmpty(_generateScriptWarning))
                    EditorGUILayout.HelpBox(_generateScriptWarning, MessageType.Warning);

                using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(_generateScriptWarning)))
                {
                    if (GUILayout.Button("生成"))
                        GenerateMasterAndMasterDataScript(_editMasterConfig, true);
                }

                if (GUILayout.Button("一括生成"))
                    DownloadAndExportSheetAllAsync(SpreadSheetId, _token);
            }
        }

        #endregion draw_window


        private void ValidationSheetData()
        {
            _downloadSheetWarning = string.Empty;

            if (string.IsNullOrEmpty(_spreadSheetId))
                _downloadSheetWarning = "スプレッドシートID を入力してください";

            if (string.IsNullOrEmpty(_sheetId))
                _downloadSheetWarning = "シートID を入力してください";

            if (string.IsNullOrEmpty(_masterName))
                _downloadSheetWarning = "マスタ名 を入力してください";
        }

        private MasterConfigData ParseCsvToConfig(string csv, string spreadSheetId, string sheetId, string masterName)
        {
            var parser = new CsvParser(_setting != null ? _setting.ignoreRowConditions : null);
            var records = parser.Parse(csv, excludeHeader: false);

            return CreateMasterConfigData(spreadSheetId, sheetId, masterName, records);
        }


        #region create_config_data

        private MasterConfigData CreateMasterConfigData(string spreadSheetId, string sheetId, string masterName,
            IReadOnlyList<IReadOnlyList<string>> records)
        {
            var config = new MasterConfigData
            {
                masterName = StringUtility.SnakeToUpperCamel(masterName + "Master"),
                spreadSheetId = spreadSheetId,
                sheetId = sheetId
            };

            if (records == null || records.Count == 0)
                return config;

            var columnNameList = records[0];
            var columnDataList = records.Count > 1 ? records[1] : null;
            var columnCount = columnNameList.Count;
            if (columnCount == 0)
                return config;

            config.columns = new MasterColumnConfigData[columnCount];
            for (var i = 0; i < columnCount; i++)
            {
                var columnData = (columnDataList != null && columnDataList.Count > i)
                    ? columnDataList[i]
                    : string.Empty;
                var columnName = columnNameList[i];
                var column = CreateMasterConfigColumnData(columnName, columnData);
                if (columnName == "id")
                    config._idMasterColumnConfigData = column;
                config.columns[i] = column;
            }

            return config;
        }

        private MasterColumnConfigData CreateMasterConfigColumnData(string columnName, string data)
        {
            var exportFlag = IsExportColumn(columnName);
            if (!exportFlag)
            {
                return new MasterColumnConfigData()
                {
                    exportFlag = false,
                };
            }

            var column = new MasterColumnConfigData()
            {
                validFlag = true,
                exportFlag = true,
                propertyName = StringUtility.Convert(columnName, _setting.columnNameNamingConvention,
                    _setting.propertyNamingConvention),
                constantName = StringUtility.Convert("column_" + columnName, _setting.columnNameNamingConvention,
                    _setting.constantNamingConvention),
                type = GetDataTypeFromString(data),
                enumType = null,
                enumTypeName = string.Empty
            };
            return column;
        }

        private bool IsExportColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return false;

            if (_setting == null)
                return columnName.IndexOf("#", StringComparison.Ordinal) == -1;

            return _setting.ignoreColumnConditions.All(ignoreColumnCondition =>
                !ignoreColumnCondition.IsIgnore(columnName));
        }

        private DataType GetDataTypeFromString(string dataString)
        {
            if (string.IsNullOrEmpty(dataString))
                return DataType.String;

            if (int.TryParse(dataString, out var intValue))
                return DataType.Int;
            if (float.TryParse(dataString, out var floatValue))
                return DataType.Float;
            if (bool.TryParse(dataString, out var boolValue))
                return DataType.Bool;

            return DataType.String;
        }

        #endregion create_config_data


        #region generate_master_script

        private void ValidationGenerateScript()
        {
            _generateScriptWarning = string.Empty;
            if (_editMasterConfig._idMasterColumnConfigData == null)
            {
                _generateScriptWarning = "\"id\"カラムを設定してください。";
                return;
            }

            if (ExportDirectoryPath.IndexOf("Assets/", StringComparison.Ordinal) != 0)
            {
                _generateScriptWarning = "生成先フォルダは \"Assets/\"から始まるパスを指定してください。";
                return;
            }

            if (StringUtility.IsExistInvalidPathChars(_directoryPath))
            {
                _generateScriptWarning = "生成先フォルダのパスに使用できない文字が含まれています。";
                return;
            }

            var fileName = $"{_editMasterConfig.masterName}.cs";
            if (StringUtility.IsExistInvalidFileNameChars(fileName))
            {
                _generateScriptWarning = "ファイル名に使用できない文字が含まれています。";
                return;
            }
        }

        private string GetAssetsPath(string fullPath)
        {
            var startIndex = fullPath.IndexOf("Assets/", System.StringComparison.Ordinal);
            if (startIndex == -1) startIndex = fullPath.IndexOf("Assets\\", System.StringComparison.Ordinal);
            if (startIndex == -1) return "";

            var assetPath = fullPath.Substring(startIndex);
            return assetPath;
        }

        private void GenerateMasterAndMasterDataScript(MasterConfigData configData, bool withRefresh)
        {
            CreateDirectoryIfNeeded(ExportDirectoryPath);

            GenerateMasterScript(ExportDirectoryPath, configData, NamespaceName);
            GenerateMasterDataScript(ExportDirectoryPath, configData, NamespaceName);

            if (withRefresh)
                AssetDatabase.Refresh();
        }

        private void CreateDirectoryIfNeeded(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        private void GenerateMasterScript(string directoryPath, MasterConfigData configData, string namespaceName)
        {
            var exportPath = $"{directoryPath}/{configData.masterName}.cs";
            var builder = new MasterScriptContentBuilder();
            File.WriteAllText(exportPath, builder.Build(configData, namespaceName));
        }

        private void GenerateMasterDataScript(string directoryPath, MasterConfigData configData, string namespaceName)
        {
            var exportPath = $"{directoryPath}/{configData.masterDataName}.cs";
            var builder = new MasterDataScriptContentBuilder();
            File.WriteAllText(exportPath, builder.Build(configData, namespaceName));
        }

        #endregion export_master_script
    } // class SpreadSheetMasterWindow
} // namespace SpreadSheetMaster.Editor