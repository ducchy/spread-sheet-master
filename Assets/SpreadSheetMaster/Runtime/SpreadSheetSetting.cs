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

		[Header("シート設定")]
		[SerializeField] private string _spreadSheetId;
		[SerializeField] private SheetData[] _sheetDataArray;
		[SerializeField] private IgnoreColumnCondition[] _ignoreColumnConditions;
		[SerializeField] private IgnoreRowCondition[] _ignoreRowConditions;

		[Header("コード設定")]
		[SerializeField] private string[] _findNamespaceNameList;
		[SerializeField] private string[] _findAssemblyNameList = { "Assembly-CSharp.dll", };
		[SerializeField] private NamingConvention _constantNamingConvention = NamingConvention.UpperCamelCase;
		[SerializeField] private NamingConvention _propertyNamingConvention = NamingConvention.UpperCamelCase;
		[SerializeField] private NamingConvention _columnNameNamingConvention = NamingConvention.SnakeCase;

		[Header("出力設定")]
		[SerializeField] private string _exportNamespaceName = string.Empty;
		[SerializeField] private string _exportScriptDirectoryPath = "Assets/";
		[SerializeField] private string _exportCsvDirectoryPath = "Assets/";

		[Header("デバッグ設定")]
		[SerializeField] private LogLevel _logLevel = LogLevel.Log;

		#endregion

		#region Variables

		public string SpreadSheetId => _spreadSheetId;
		public SheetData[] SheetDataArray => _sheetDataArray;
		public IgnoreColumnCondition[] IgnoreColumnConditions => _ignoreColumnConditions;
		public IgnoreRowCondition[] IgnoreRowConditions => _ignoreRowConditions;

		public string[] FindNamespaceNameList => _findNamespaceNameList;
		public string[] FindAssemblyNameList => _findAssemblyNameList;
		public NamingConvention ConstantNamingConvention => _constantNamingConvention;
		public NamingConvention PropertyNamingConvention => _propertyNamingConvention;
		public NamingConvention ColumnNameNamingConvention => _columnNameNamingConvention;

		public string ExportNamespaceName => _exportNamespaceName;
		public string ExportScriptDirectoryPath => _exportScriptDirectoryPath;
		public string ExportCsvDirectoryPath => _exportCsvDirectoryPath;

		public LogLevel LogLevel => _logLevel;

		#endregion

		#region Methods

		/// <summary> シートデータを取得 </summary>
		public SheetData GetSheetData(int index) {
			return 0 <= index && index < _sheetDataArray.Length
				? _sheetDataArray[index]
				: null;
		}

		#endregion
	}
}