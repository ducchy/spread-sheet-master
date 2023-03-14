using System;
using UnityEngine;

namespace SpreadSheetMaster
{
    [Serializable]
    public enum NamingConvention
    {
        LowerCamelCase, // eXAMPLEnAME
        UpperCamelCase, // ExampleName
        SnakeCase, // example_name
        UpperSnakeCase, // EXAMPLE_NAME
        KebabCase, // example-name
    }

    [CreateAssetMenu(fileName = "SpreadSheetSetting", menuName = "SpreadSheetMaster/ImportSetting")]
    public class SpreadSheetSetting : ScriptableObject
    {
        public SpreadSheetData[] spreadSheetDatas;
        public SheetData[] sheetDatas;
        public NamingConvention columnNameNamingConvention { get; private set; } = NamingConvention.SnakeCase;
        public NamingConvention variableNamingConvention { get; private set; } = NamingConvention.UpperCamelCase;
        public IgnoreColumnCondition[] ignoreColumnConditions;
        public IgnoreRowCondition[] ignoreRowConditions;
        public string exportDirectoryPath = "Assets/";
        public string namespaceName = string.Empty;
    }
}