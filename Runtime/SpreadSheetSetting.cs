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
    
    [CreateAssetMenu(fileName = "SpreadSheetSetting", menuName = "SpreadSheetMaster/ImportSetting")]
    public class SpreadSheetSetting : ScriptableObject
    {
        [Header("シート情報")]
        public SpreadSheetData[] spreadSheetDataArray;
        public SheetData[] sheetDataArray;
        public IgnoreColumnCondition[] ignoreColumnConditions;
        public IgnoreRowCondition[] ignoreRowConditions;
        public NamingConvention columnNameNamingConvention { get; private set; } = NamingConvention.SnakeCase;
        
        [Header("コード情報")]
        public string namespaceName = string.Empty;
        public NamingConvention constantNamingConvention = NamingConvention.UpperCamelCase;
        public NamingConvention propertyNamingConvention = NamingConvention.UpperCamelCase;

        [Header("出力情報")]
        public string exportScriptDirectoryPath = "Assets/";
        // public string exportCsvDirectoryPath = "Assets/";
    }
}