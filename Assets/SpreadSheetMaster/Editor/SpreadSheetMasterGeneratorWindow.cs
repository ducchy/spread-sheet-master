using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace SpreadSheetMaster.Editor
{
    /// <summary> スプレッドシートマスタ生成ウィンドウ </summary>
    public class SpreadSheetMasterGeneratorWindow : EditorWindow
    {
        private readonly SheetUrlBuilder _sheetUrlBuilder = new();
        private readonly CancellationTokenSource _cts = new();

        [SerializeField] private SpreadSheetSetting _setting;
        [SerializeField] private int _sheetIndex;
        [SerializeField] private string _overwriteSpreadSheetId;
        [SerializeField] private string _overwriteSheetId;
        [SerializeField] private MasterConfigData _editMasterConfig;

        private bool _downloadingFlag;
        private bool _batchDownloadingFlag;
        private string _downloadText;
        private string _downloadSheetWarning;
        private string _downloadSheetError;
        private string _generateScriptWarning;
        private string _generateCsvWarning;
        private Vector2 _scrollPositionCsvPreview;
        private Vector2 _scrollPositionColumns;

        private CancellationToken Token => _cts.Token;

        private string SpreadSheetId => !string.IsNullOrEmpty(_overwriteSpreadSheetId)
            ? _overwriteSpreadSheetId
            : _setting.spreadSheetId;

        private SheetData SheetData => _setting.GetSheetData(_sheetIndex);

        private string SheetId => !string.IsNullOrEmpty(_overwriteSheetId)
            ? _overwriteSheetId
            : SheetData != null
                ? SheetData.id
                : string.Empty;

        private string SheetName => SheetData != null ? SheetData.name : string.Empty;
        private string SheetMasterName => SheetData != null ? SheetData.masterName : string.Empty;

        private string ExportNamespaceName => _setting.exportNamespaceName;
        private string ExportScriptDirectoryPath => _setting.exportScriptDirectoryPath;
        private string ExportCsvDirectoryPath => _setting.exportCsvDirectoryPath;

        /// <summary> ウィンドウを開く </summary>
        [MenuItem("Window/Spread Sheet Master Generator")]
        public static void ShowSpreadSheetMasterWindow()
        {
            var window = GetWindow<SpreadSheetMasterGeneratorWindow>();
            window.titleContent = new GUIContent("Spread Sheet Master Generator");
            window.Focus();
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

            using (new EditorGUI.DisabledScope(_downloadingFlag || _batchDownloadingFlag))
            {
                // インポート設定
                DrawImportSetting();

                if (_setting == null)
                    EditorGUILayout.HelpBox("シート設定を設定してください", MessageType.Warning);
                else
                {
                    // シートのダウンロード
                    DrawDownloadSheet();

                    EditorGUILayout.Space();

                    // マスタ構成の編集
                    DrawEditMasterConfig();

                    EditorGUILayout.Space();

                    // マスタスクリプト生成
                    DrawGenerateMasterScript();

                    // csv生成
                    DrawGenerateCsvScript();
                }
            }

            GUILayout.FlexibleSpace();
        }

        private void DrawImportSetting()
        {
            EditorGUILayout.LabelField("シート設定", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Undo.RecordObject(this, "Modify ImportSetting");
                _setting =
                    (SpreadSheetSetting)EditorGUILayout.ObjectField(_setting,
                        typeof(SpreadSheetSetting), false);
            }
        }

        private void DrawDownloadSheet()
        {
            EditorGUILayout.LabelField("シートのダウンロード", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Undo.RecordObject(this, "Modify SpreadSheetId or SheetName");

                _sheetIndex = EditorGUILayout.Popup("シート", _sheetIndex,
                    _setting.sheetDataArray.Select(sd => sd.name).ToArray());

                EditorGUILayout.LabelField("マスタ名", SheetMasterName);

                _overwriteSpreadSheetId =
                    EditorGUILayout.TextField("上書きスプレッドシートID", _overwriteSpreadSheetId);
                _overwriteSheetId = EditorGUILayout.TextField("上書きシートID", _overwriteSheetId);

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

                        DownloadSheetAsync(Token);
                    }

                    if (GUILayout.Button("ブラウザで開く"))
                        Application.OpenURL(_sheetUrlBuilder.BuildEditUrl(SpreadSheetId, SheetId));
                }

                if (!string.IsNullOrEmpty(_downloadSheetError))
                    EditorGUILayout.HelpBox(_downloadSheetError, MessageType.Error);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox,
                           GUILayout.Height(120)))
                {
                    using (var scroll =
                           new EditorGUILayout.ScrollViewScope(_scrollPositionCsvPreview))
                    {
                        _scrollPositionCsvPreview = scroll.scrollPosition;
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.TextArea(
                                _downloadText == string.Empty
                                    ? "ダウンロードしたシートの内容が表示されます"
                                    : _downloadText,
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
                    EditorGUILayout.LabelField("マスタ名", GUILayout.MaxWidth(80));
                    EditorGUILayout.TextField(_editMasterConfig.masterName);
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("", EditorStyles.boldLabel,
                            GUILayout.MaxWidth(20));
                        EditorGUILayout.LabelField("使用", EditorStyles.boldLabel,
                            GUILayout.MaxWidth(40));
                        EditorGUILayout.LabelField("カラム名", EditorStyles.boldLabel,
                            GUILayout.MaxWidth(200));
                        EditorGUILayout.LabelField("データ型", EditorStyles.boldLabel,
                            GUILayout.MaxWidth(80));
                        EditorGUILayout.LabelField("Enum名", EditorStyles.boldLabel,
                            GUILayout.MaxWidth(280));

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
                                EditorGUILayout.LabelField(column.validFlag ? "○" : "×",
                                    GUILayout.MaxWidth(20));
                                column.exportFlag = EditorGUILayout.Toggle(column.exportFlag,
                                    GUILayout.MaxWidth(40));
                                EditorGUILayout.LabelField(column.propertyName,
                                    GUILayout.MaxWidth(200));
                                var dataType =
                                    (DataType)EditorGUILayout.EnumPopup(column.type,
                                        GUILayout.MaxWidth(80));

                                using (new EditorGUI.DisabledScope(column.type != DataType.Enum))
                                {
                                    column.enumTypeName =
                                        EditorGUILayout.TextField(column.enumTypeName,
                                            GUILayout.MaxWidth(240));

                                    if (GUILayout.Button("適用", GUILayout.MaxWidth(40)))
                                    {
                                        var type = FindEnumType(column.enumTypeName);
                                        column.validFlag = type != null;
                                        column.enumType = type;
                                        column.enumTypeName = type?.Name;
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
                                        column.enumType = null;
                                        column.enumTypeName = string.Empty;
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

                EditorGUILayout.LabelField("生成先フォルダ", ExportScriptDirectoryPath);
                EditorGUILayout.LabelField("名前空間", ExportNamespaceName);

                ValidationGenerateScript();

                if (!string.IsNullOrEmpty(_generateScriptWarning))
                    EditorGUILayout.HelpBox(_generateScriptWarning, MessageType.Warning);

                using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(_generateScriptWarning)))
                {
                    if (GUILayout.Button("生成"))
                        GenerateMasterAndMasterDataScript(_editMasterConfig, true);
                }

                if (GUILayout.Button("一括生成"))
                    DownloadAndExportScriptAllAsync(SpreadSheetId, Token);
            }
        }

        private void DrawGenerateCsvScript()
        {
            EditorGUILayout.LabelField("csv生成", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (string.IsNullOrEmpty(_downloadText))
                {
                    EditorGUILayout.LabelField("シートをダウンロードしてください");
                    return;
                }

                EditorGUILayout.LabelField("生成先フォルダ", ExportCsvDirectoryPath);

                ValidationGenerateCsv();

                if (!string.IsNullOrEmpty(_generateCsvWarning))
                    EditorGUILayout.HelpBox(_generateCsvWarning, MessageType.Warning);

                using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(_generateCsvWarning)))
                {
                    if (GUILayout.Button("生成"))
                        GenerateMasterCsv(_editMasterConfig, true);
                }

                if (GUILayout.Button("一括生成"))
                    DownloadAndExportCsvAllAsync(SpreadSheetId, Token);
            }
        }

        #endregion draw_window


        #region download_sheet

        private async void DownloadSheetAsync(CancellationToken token)
        {
            await DownloadSheetAsync(SpreadSheetId, SheetId, SheetName, SheetMasterName, token);
        }

        private async Task DownloadSheetAsync(string spreadSheetId, string sheetId,
            string sheetName,
            string sheetMasterName,
            CancellationToken token)
        {
            var downloader = new SheetDownloader();
            var downloadSheetAsync = downloader.DownloadSheetAsync(spreadSheetId, sheetId, token);

            while (!downloadSheetAsync.IsDone)
                await Task.Yield();

            if (downloadSheetAsync.Exception != null)
            {
                OnDownloadError(downloadSheetAsync.Exception.Message);
                return;
            }

            OnDownloadSuccess(downloadSheetAsync.Result, sheetName, sheetMasterName);
        }

        private void OnDownloadSuccess(string response, string sheetName,
            string sheetMasterName)
        {
            _downloadText = response;
            _editMasterConfig = ParseCsvToConfig(_downloadText, sheetName, sheetMasterName);
            _downloadingFlag = false;
        }

        private void OnDownloadError(string error)
        {
            _downloadSheetError = error;
            _downloadingFlag = false;
        }

        private void ValidationSheetData()
        {
            _downloadSheetWarning = string.Empty;

            if (string.IsNullOrEmpty(SpreadSheetId))
                _downloadSheetWarning = "スプレッドシートID を入力してください";

            if (string.IsNullOrEmpty(SheetId))
                _downloadSheetWarning = "シートID を入力してください";

            if (string.IsNullOrEmpty(SheetName))
                _downloadSheetWarning = "シート名 を入力してください";

            if (string.IsNullOrEmpty(SheetMasterName))
                _downloadSheetWarning = "マスタ名 を入力してください";
        }

        #endregion download_sheet


        #region create_config_data

        private MasterConfigData ParseCsvToConfig(
            string csv,
            string sheetName,
            string masterName)
        {
            ICsvParser parser = new CsvParser(_setting.ignoreRowConditions);
            var records = parser.Parse(csv, excludeHeader: false);

            return CreateMasterConfigData(sheetName, masterName, records);
        }

        private MasterConfigData CreateMasterConfigData(
            string sheetName,
            string masterName,
            IReadOnlyList<IReadOnlyList<string>> records)
        {
            var config = new MasterConfigData
            {
                masterName = StringUtility.SnakeToUpperCamel(masterName + "Master"),
                sheetName = sheetName,
                exportNamespaceName = ExportNamespaceName,
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
                    config.idMasterColumnConfigData = column;
                config.columns[i] = column;
            }

            config.maxMasterColumnConfigData =
                CreateMasterConfigColumnData("max", columnCount.ToString());

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
                propertyName = StringUtility.Convert(columnName,
                    _setting.columnNameNamingConvention,
                    _setting.propertyNamingConvention),
                constantName = StringUtility.Convert("column_" + columnName,
                    _setting.columnNameNamingConvention,
                    _setting.constantNamingConvention),
                type = GetDataTypeFromString(data),
                enumType = null,
                enumTypeName = string.Empty,
            };
            return column;
        }

        private bool IsExportColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return false;

            return _setting.ignoreColumnConditions.All(ignoreColumnCondition =>
                !ignoreColumnCondition.IsIgnore(columnName));
        }

        private DataType GetDataTypeFromString(string dataString)
        {
            if (string.IsNullOrEmpty(dataString))
                return DataType.String;

            if (int.TryParse(dataString, out _))
                return DataType.Int;
            if (float.TryParse(dataString, out _))
                return DataType.Float;
            if (bool.TryParse(dataString, out _))
                return DataType.Bool;

            return DataType.String;
        }

        private Type FindEnumType(string enumTypeName)
        {
            foreach (var namespaceName in _setting.findNamespaceNameList)
            {
                foreach (var assemblyName in _setting.findAssemblyNameList)
                {
                    var type = Type.GetType($"{namespaceName}.{enumTypeName}, {assemblyName}");
                    if (type is { IsEnum: true })
                        return type;
                }
            }

            return null;
        }

        #endregion create_config_data


        #region generate_master_script

        private void ValidationGenerateScript()
        {
            _generateScriptWarning = string.Empty;
            if (_editMasterConfig.idMasterColumnConfigData == null)
            {
                _generateScriptWarning = "\"id\"カラムを設定してください。";
                return;
            }

            if (ExportScriptDirectoryPath.IndexOf("Assets/", StringComparison.Ordinal) != 0)
            {
                _generateScriptWarning = "生成先フォルダは \"Assets/\"から始まるパスを指定してください。";
                return;
            }

            if (StringUtility.IsExistInvalidPathChars(ExportScriptDirectoryPath))
            {
                _generateScriptWarning = "生成先フォルダのパスに使用できない文字が含まれています。";
                return;
            }

            var fileName = $"{_editMasterConfig.masterName}.cs";
            if (StringUtility.IsExistInvalidFileNameChars(fileName))
                _generateScriptWarning = "ファイル名に使用できない文字が含まれています。";
        }

        private void GenerateMasterAndMasterDataScript(MasterConfigData configData,
            bool withRefresh)
        {
            CreateDirectoryIfNeeded(ExportScriptDirectoryPath);

            GenerateMasterScript(ExportScriptDirectoryPath, configData);
            GenerateMasterDataScript(ExportScriptDirectoryPath, configData);

            if (withRefresh)
                AssetDatabase.Refresh();
        }

        private void CreateDirectoryIfNeeded(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        private void GenerateMasterScript(string directoryPath, MasterConfigData configData)
        {
            var exportPath = $"{directoryPath}/{configData.masterName}.cs";
            var builder = new MasterScriptContentBuilder();
            var contents = builder.Build(configData);
            File.WriteAllText(exportPath, contents);
        }

        private void GenerateMasterDataScript(string directoryPath, MasterConfigData configData)
        {
            var exportPath = $"{directoryPath}/{configData.masterDataName}.cs";
            var builder = new MasterDataScriptContentBuilder();
            var contents = builder.Build(configData);
            File.WriteAllText(exportPath, contents);
        }

        private async void DownloadAndExportScriptAllAsync(string spreadSheetId,
            CancellationToken token)
        {
            _batchDownloadingFlag = true;

            var sheetDataList = _setting.sheetDataArray;
            foreach (var sheetData in sheetDataList)
            {
                await DownloadSheetAsync(spreadSheetId, sheetData.id, sheetData.name,
                    sheetData.masterName, token);

                ValidationGenerateScript();
                if (!string.IsNullOrEmpty(_generateScriptWarning))
                    continue;

                GenerateMasterAndMasterDataScript(_editMasterConfig, false);
            }

            AssetDatabase.Refresh();

            _batchDownloadingFlag = false;
        }

        #endregion export_master_script


        #region generate_master_csv

        private void ValidationGenerateCsv()
        {
            _generateCsvWarning = string.Empty;

            if (ExportCsvDirectoryPath.IndexOf("Assets/", StringComparison.Ordinal) != 0)
            {
                _generateCsvWarning = "生成先フォルダは \"Assets/\"から始まるパスを指定してください。";
                return;
            }

            if (StringUtility.IsExistInvalidPathChars(ExportCsvDirectoryPath))
            {
                _generateCsvWarning = "生成先フォルダのパスに使用できない文字が含まれています。";
                return;
            }

            var fileName = $"{_editMasterConfig.masterName}.csv";
            if (StringUtility.IsExistInvalidFileNameChars(fileName))
                _generateCsvWarning = "ファイル名に使用できない文字が含まれています。";
        }

        private void GenerateMasterCsv(MasterConfigData configData, bool withRefresh)
        {
            CreateDirectoryIfNeeded(ExportCsvDirectoryPath);

            var exportPath = $"{ExportCsvDirectoryPath}/{configData.masterName}.csv";
            File.WriteAllText(exportPath, _downloadText);

            if (withRefresh)
                AssetDatabase.Refresh();
        }

        private async void DownloadAndExportCsvAllAsync(string spreadSheetId,
            CancellationToken token)
        {
            _batchDownloadingFlag = true;

            var sheetDataList = _setting.sheetDataArray;
            foreach (var sheetData in sheetDataList)
            {
                await DownloadSheetAsync(spreadSheetId, sheetData.id, sheetData.name,
                    sheetData.masterName, token);

                ValidationGenerateCsv();
                if (!string.IsNullOrEmpty(_generateCsvWarning))
                    continue;

                GenerateMasterCsv(_editMasterConfig, false);
            }

            AssetDatabase.Refresh();

            _batchDownloadingFlag = false;
        }

        #endregion export_master_script
    } // class SpreadSheetMasterWindow
} // namespace SpreadSheetMaster.Editor