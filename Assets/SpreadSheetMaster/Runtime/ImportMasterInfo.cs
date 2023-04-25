using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace SpreadSheetMaster
{
    public class ImportMasterInfo
    {
        private readonly StringBuilder _sb = new();

        private LogLevel _logLevel;

        private LogLevel _maxLogLevel;

        [Conditional("SSM_LOG")]
        public void Initialize(string className, string key, int count, LogLevel logLevel)
        {
            _logLevel = logLevel;

            _maxLogLevel = LogLevel.Log;

            _sb.Clear();

            _sb.AppendLine($"[{className}(key={key})]")
                .AppendLine($"count={count}")
                .AppendLine();
        }

        [Conditional("SSM_LOG")]
        public void Imported(string data)
        {
            AppendLog($"Imported {data}");
        }

        [Conditional("SSM_LOG")]
        public void CastOnParse(string log)
        {
            AppendWarning(log);
        }

        [Conditional("SSM_LOG")]
        public void FailedParse(string type, int index, string str)
        {
            AppendError($"{type}へのパースに失敗 (index={index}, str={str}");
        }

        [Conditional("SSM_LOG")]
        public void OutOfRangeIndex(int index)
        {
            AppendError($"範囲外のindex指定(index={index})");
        }

        private void AppendLog(string log)
        {
            if (_logLevel > LogLevel.Log)
                return;

            _sb.AppendLine(log);
        }

        private void AppendWarning(string warning)
        {
            if (_logLevel > LogLevel.Warning)
                return;

            _sb.AppendLine($"<color=#ffff00>[Warning] {warning}</color>");

            if (_maxLogLevel < LogLevel.Warning)
                _maxLogLevel = LogLevel.Warning;
        }
        
        private void AppendError(string error)
        {
            if (_logLevel > LogLevel.Error)
                return;

            _sb.AppendLine($"<color=#ff0000>[Error] {error}</color>");

            if (_maxLogLevel < LogLevel.Error)
                _maxLogLevel = LogLevel.Error;
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