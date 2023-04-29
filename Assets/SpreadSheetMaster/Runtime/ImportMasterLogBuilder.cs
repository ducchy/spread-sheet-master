using System;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace SpreadSheetMaster
{
    public class ImportMasterLogBuilder
    {
        private readonly StringBuilder _sb = new();

        private LogLevel _logLevel;
        private LogLevel _currentDataMaxLogLevel;
        private LogLevel _maxLogLevel;

        [Conditional("SSM_LOG")]
        public void Initialize(string className, string key, int count, LogLevel logLevel)
        {
            _logLevel = logLevel;

            _maxLogLevel = LogLevel.Log;
            _currentDataMaxLogLevel = LogLevel.Log;

            if (_logLevel == LogLevel.None)
                return;

            _sb.Clear();
            _sb.AppendLine($"[{className}(key={key})]");
            _sb.AppendLine($"count={count}");
            _sb.AppendLine();
        }
        
        [Conditional("SSM_LOG")]
        public void ImportedData(string data)
        {
            if (_logLevel == LogLevel.None)
                return;
            
            if (_currentDataMaxLogLevel >= _logLevel)
                _sb.AppendLine($"Imported {data}");

            _currentDataMaxLogLevel = LogLevel.Log;
        }

        [Conditional("SSM_LOG")]
        public void CastOnParse(string log)
        {
            AppendWarning(log);
        }

        [Conditional("SSM_LOG")]
        public void FailedParse(string type, int index, string str)
        {
            AppendError($"{type}へのパースに失敗(index={index}, str={str}");
        }

        [Conditional("SSM_LOG")]
        public void OutOfRangeIndex(int index)
        {
            AppendError($"範囲外のindex指定(index={index})");
        }

        [Conditional("SSM_LOG")]
        public void DuplicateKey(int id)
        {
            AppendError($"IDが重複(id={id})");
        }

        private void AppendWarning(string warning)
        {
            AppendLog(warning, LogLevel.Warning);
        }

        private void AppendError(string error)
        {
            AppendLog(error, LogLevel.Error);
        }

        private void AppendLog(string log, LogLevel logLevel = LogLevel.Log)
        {
            if (logLevel == LogLevel.None || _logLevel > logLevel)
                return;

            switch (logLevel)
            {
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

            if (_maxLogLevel < logLevel)
                _maxLogLevel = logLevel;

            if (_currentDataMaxLogLevel < logLevel)
                _currentDataMaxLogLevel = logLevel;
        }

        [Conditional("SSM_LOG")]
        public void Exception(Exception e)
        {
            AppendLog($"Exception発生によりインポート中断(Exception={e})", LogLevel.Error);
            ExportLog();
        }

        [Conditional("SSM_LOG")]
        public void ExportLog()
        {
            if (_maxLogLevel < _logLevel)
                return;

            switch (_maxLogLevel)
            {
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
    }
}