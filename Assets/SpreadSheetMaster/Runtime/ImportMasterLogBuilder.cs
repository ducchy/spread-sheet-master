using System;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace SpreadSheetMaster {
	/// <summary> マスタインポート時のログ作成 </summary>
	public class ImportMasterLogBuilder {
		#region Variables

		/// <summary> 文字列ビルダー </summary>
		private readonly StringBuilder _sb = new();

		/// <summary> ログレベル </summary>
		private LogLevel _logLevel;

		/// <summary> 現在のデータの最大ログレベル </summary>
		private LogLevel _currentDataMaxLogLevel;

		/// <summary> 最大ログレベル </summary>
		private LogLevel _maxLogLevel;

		#endregion

		#region Methods

		/// <summary> 初期化 </summary>
		[Conditional("SSM_LOG")]
		public void Initialize(string className, int count, LogLevel logLevel) {
			_logLevel = logLevel;

			_maxLogLevel = LogLevel.Log;
			_currentDataMaxLogLevel = LogLevel.Log;

			if (_logLevel == LogLevel.None) {
				return;
			}

			_sb.Clear();
			_sb.AppendLine($"[{className}]");
			_sb.AppendLine($"count={count}");
			_sb.AppendLine();
		}

		/// <summary> データインポート </summary>
		[Conditional("SSM_LOG")]
		public void ImportedData(string data) {
			if (_logLevel == LogLevel.None) {
				return;
			}

			if (_currentDataMaxLogLevel >= _logLevel) {
				_sb.AppendLine($"Imported {data}");
			}

			_currentDataMaxLogLevel = LogLevel.Log;
		}

		/// <summary> パース時のキャスト </summary>
		[Conditional("SSM_LOG")]
		public void CastOnParse(string log) {
			AppendWarning(log);
		}

		/// <summary> パース失敗 </summary>
		[Conditional("SSM_LOG")]
		public void FailedParse(string type, int index, string str) {
			AppendError($"{type}へのパースに失敗(index={index}, str={str}");
		}

		/// <summary> 範囲外インデックス参照 </summary>
		[Conditional("SSM_LOG")]
		public void OutOfRangeIndex(int index) {
			AppendError($"範囲外のindex指定(index={index})");
		}

		/// <summary> キー重複 </summary>
		[Conditional("SSM_LOG")]
		public void DuplicateKey(int id) {
			AppendError($"IDが重複(id={id})");
		}

		/// <summary> エクセプション </summary>
		[Conditional("SSM_LOG")]
		public void Exception(Exception e) {
			AppendLog($"Exception発生によりインポート中断(Exception={e})", LogLevel.Error);
			ExportLog();
		}

		/// <summary> ログ出力 </summary>
		[Conditional("SSM_LOG")]
		public void ExportLog() {
			if (_maxLogLevel < _logLevel) {
				return;
			}

			switch (_maxLogLevel) {
				case LogLevel.Log:
					Debug.Log(_sb.ToString());
					break;
				case LogLevel.Warning:
					Debug.LogWarning(_sb.ToString());
					break;
				case LogLevel.Error:
					Debug.LogError(_sb.ToString());
					break;
			}
		}

		/// <summary> 警告ログ追加 </summary>
		private void AppendWarning(string warning) {
			AppendLog(warning, LogLevel.Warning);
		}

		/// <summary> エラーログ追加 </summary>
		private void AppendError(string error) {
			AppendLog(error, LogLevel.Error);
		}

		/// <summary> ログ追加 </summary>
		private void AppendLog(string log, LogLevel logLevel = LogLevel.Log) {
			if (logLevel == LogLevel.None || _logLevel > logLevel) {
				return;
			}

			switch (logLevel) {
				case LogLevel.Warning:
					_sb.AppendLine($"<color=#ffff00>[Warning] {log}</color>");
					break;
				case LogLevel.Error:
					_sb.AppendLine($"<color=#ff0000>[Error] {log}</color>");
					break;
				case LogLevel.Log:
					_sb.AppendLine(log);
					break;
				default:
					return;
			}

			if (_maxLogLevel < logLevel) {
				_maxLogLevel = logLevel;
			}

			if (_currentDataMaxLogLevel < logLevel) {
				_currentDataMaxLogLevel = logLevel;
			}
		}

		#endregion
	}
}