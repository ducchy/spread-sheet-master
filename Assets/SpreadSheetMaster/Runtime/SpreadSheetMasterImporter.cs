using System;
using System.Threading;

namespace SpreadSheetMaster
{
    /// <summary> スプレッドシートインポート </summary>
    public class SpreadSheetMasterImporter
    {
        /// <summary> csvパーサー </summary>
        private readonly ICsvParser _parser;

        /// <summary> ログレベル </summary>
        private readonly LogLevel _logLevel;

        /// <summary> コンストラクタ </summary>
        public SpreadSheetMasterImporter(
            ICsvParser parser,
            LogLevel logLevel)
        {
            _parser = parser;
            _logLevel = logLevel;
        }

        /// <summary> インポート </summary>
        public AsyncOperationHandle<ImportMasterLogBuilder> ImportAsync(
            IImportableSpreadSheetMaster master,
            string csv,
            CancellationToken token)
        {
            var op = new AsyncOperator<ImportMasterLogBuilder>();

            ImportFromCsv(op, master, csv);

            return op;
        }

        /// <summary> CSVからインポート </summary>
        private void ImportFromCsv(
            AsyncOperator<ImportMasterLogBuilder> op,
            IImportableSpreadSheetMaster master,
            string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                op.Canceled(new InvalidOperationException("csv is empty"));
                return;
            }

            // パース
            var records = _parser.Parse(csv, excludeHeader: true);

            // インポート
            var importInfo = new ImportMasterLogBuilder();
            importInfo.Initialize(master.className, records.Count, _logLevel);
            try
            {
                master.Import(records, importInfo);
            }
            catch (Exception e)
            {
                // インポート失敗
                importInfo.Exception(e);
                op.Canceled(e);
                return;
            }

            // インポート成功
            importInfo.ExportLog();
            op.Completed(importInfo);
        }
    }
}