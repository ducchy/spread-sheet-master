using System;
using UnityEngine;

namespace SpreadSheetMaster
{
    [Serializable]
    public enum NamingConvention
    {
        /// <summary>
        /// exampleName
        /// </summary>
        LowerCamelCase,

        /// <summary>
        /// ExampleName
        /// </summary>
        UpperCamelCase,

        /// <summary>
        /// example_name
        /// </summary>
        SnakeCase,

        /// <summary>
        /// EXAMPLE_NAME
        /// </summary>
        UpperSnakeCase,

        /// <summary>
        /// example-name
        /// </summary>
        KebabCase,
    }

    [Serializable]
    public enum SheetDownloadKey
    {
        SheetId,
        SheetName,
    }

    public enum LogLevel
    {
        Log,
        Warning,
        Error,
        None,
    }

    public enum ImportSource
    {
        SpreadSheet,
        ResourceCsv,
    }

    [CreateAssetMenu(fileName = "SpreadSheetSetting", menuName = "SpreadSheetMaster/ImportSetting")]
    public class SpreadSheetSetting : ScriptableObject
    {
        [Header("シート設定")] public SpreadSheetData[] spreadSheetDataArray;
        public SheetData[] sheetDataArray;
        public IgnoreColumnCondition[] ignoreColumnConditions;
        public IgnoreRowCondition[] ignoreRowConditions;
        public NamingConvention columnNameNamingConvention { get; private set; } = NamingConvention.SnakeCase;

        [Header("コード設定")] public string[] findNamespaceNameList;
        public string[] findAssemblyNameList = { "Assembly-CSharp.dll" };
        public NamingConvention constantNamingConvention = NamingConvention.UpperCamelCase;
        public NamingConvention propertyNamingConvention = NamingConvention.UpperCamelCase;
        public SheetDownloadKey sheetDownloadKey;

        [Header("出力設定")] public string exportNamespaceName = string.Empty;
        public string exportScriptDirectoryPath = "Assets/";
        public string exportCsvDirectoryPath = "Assets/";

        [Header("インポート設定")] public ImportSource importSource = ImportSource.SpreadSheet;
        public string importResourceDirectoryPath = "Assets/Resources/Csv";

        [Header("デバッグ設定")] public LogLevel logLevel = LogLevel.Log;

        public SpreadSheetData GetSpreadSheetData(int index)
        {
            return (0 <= index && index < spreadSheetDataArray.Length)
                ? spreadSheetDataArray[index]
                : null;
        }

        public SheetData GetSheetData(int index)
        {
            return (0 <= index && index < sheetDataArray.Length)
                ? sheetDataArray[index]
                : null;
        }
    }
}