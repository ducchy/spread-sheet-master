using System;
using UnityEngine;

namespace SpreadSheetMaster {
	/// <summary> 命名規則 </summary>
	[Serializable]
	public enum NamingConvention {
		/// <summary>
		/// ローワーキャメルケース
		/// ex. exampleName
		/// </summary>
		LowerCamelCase,

		/// <summary>
		/// アッパーキャメルケース
		/// ex. ExampleName
		/// </summary>
		UpperCamelCase,

		/// <summary>
		/// スネークケース
		/// ex. example_name
		/// </summary>
		SnakeCase,

		/// <summary>
		/// アッパーキャメルケース
		/// ex. EXAMPLE_NAME
		/// </summary>
		UpperSnakeCase,

		/// <summary>
		/// ケバブケース
		/// ex. example-name
		/// </summary>
		KebabCase,
	}

	/// <summary> ログレベル </summary>
	public enum LogLevel {
		/// <summary> ログ </summary>
		Log,

		/// <summary> 警告 </summary>
		Warning,

		/// <summary> エラー </summary>
		Error,

		/// <summary> なし </summary>
		None,
	}

	/// <summary> スプレッドシート設定 </summary>
	[CreateAssetMenu(fileName = "SpreadSheetSetting", menuName = "SpreadSheetMaster/ImportSetting")]
	public class SpreadSheetSetting : ScriptableObject {
		#region Serialize Fields

		[Header("シート設定")] public string spreadSheetId;
		public SheetData[] sheetDataArray;
		public IgnoreColumnCondition[] ignoreColumnConditions;
		public IgnoreRowCondition[] ignoreRowConditions;

		[Header("コード設定")] public string[] findNamespaceNameList;
		public string[] findAssemblyNameList = { "Assembly-CSharp.dll", };
		public NamingConvention constantNamingConvention = NamingConvention.UpperCamelCase;
		public NamingConvention propertyNamingConvention = NamingConvention.UpperCamelCase;

		[Header("出力設定")] public string exportNamespaceName = string.Empty;
		public string exportScriptDirectoryPath = "Assets/";
		public string exportCsvDirectoryPath = "Assets/";

		[Header("デバッグ設定")] public LogLevel logLevel = LogLevel.Log;

		#endregion

		#region Variables

		public NamingConvention columnNameNamingConvention { get; private set; } = NamingConvention.SnakeCase;

		#endregion

		#region Methods

		/// <summary> シートデータを取得 </summary>
		public SheetData GetSheetData(int index) {
			return 0 <= index && index < sheetDataArray.Length
				? sheetDataArray[index]
				: null;
		}

		#endregion
	}
}