using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace SpreadSheetMaster
{
    /// <summary> スプレッドシートインポート </summary>
    public class SpreadSheetMasterImporter
    {
        /// <summary> パーサー </summary>
        private readonly CsvParser _parser;

        /// <summary> ログレベル </summary>
        private readonly LogLevel _logLevel;

        /// <summary> シートダウンローダー </summary>
        private readonly SheetDownloader _sheetDownloader = new();

        /// <summary> コンストラクタ </summary>
        public SpreadSheetMasterImporter(CsvParser parser, LogLevel logLevel)
        {
            _parser = parser;
            _logLevel = logLevel;
        }

        /// <summary> スプレッドシートからインポート </summary>
        public AsyncOperationHandle<ImportMasterLogBuilder> ImportFromSpreadSheetAsync(
            IImportableSpreadSheetMaster master,
            string spreadSheetId,
            string sheetId,
            CancellationToken token)
        {
            var op = new AsyncOperator<ImportMasterLogBuilder>();

            var sheetDownloadHandle = _sheetDownloader.DownloadSheetAsync(spreadSheetId, sheetId, token);
            ImportFromSpreadSheetInternalAsync(op, sheetDownloadHandle, master, token)
                .ContinueWith(_ => { }, token);
            return op;
        }

        /// <summary> リソースからインポート </summary>
        public AsyncOperationHandle<ImportMasterLogBuilder> ImportFromResource(
            IImportableSpreadSheetMaster master,
            string resourcePath)
        {
            var op = new AsyncOperator<ImportMasterLogBuilder>();

            var csvFile = Resources.Load<TextAsset>(resourcePath);
            if (csvFile == null)
            {
                op.Canceled(new InvalidOperationException($"Failed to load resource: path={resourcePath}"));
                return op;
            }

            return ImportFromCsv(op, master, csvFile.text);
        }

        /// <summary> スプレッドシートからインポート </summary>
        private async Task ImportFromSpreadSheetInternalAsync(AsyncOperator<ImportMasterLogBuilder> op,
            AsyncOperationHandle<string> sheetDownloadHandle,
            IImportableSpreadSheetMaster master,
            CancellationToken token)
        {
            while (!sheetDownloadHandle.IsDone)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    op.Canceled();
                    return;
                }

                await Task.Yield();
            }

            if (sheetDownloadHandle.Exception != null)
            {
                op.Canceled(sheetDownloadHandle.Exception);
                return;
            }

            var csv = sheetDownloadHandle.Result;
            ImportFromCsv(op, master, csv);
        }

        /// <summary> CSVからインポート </summary>
        private AsyncOperationHandle<ImportMasterLogBuilder> ImportFromCsv(
            AsyncOperator<ImportMasterLogBuilder> op,
            IImportableSpreadSheetMaster master,
            string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                op.Canceled(new InvalidOperationException("csv is empty"));
                return op;
            }

            var records = _parser.Parse(csv, excludeHeader: true);
            var importInfo = new ImportMasterLogBuilder();
            importInfo.Initialize(master.className, records.Count, _logLevel);
            try
            {
                master.Import(records, importInfo);
            }
            catch (Exception e)
            {
                importInfo.Exception(e);
                op.Canceled(e);
                return op;
            }

            importInfo.ExportLog();
            op.Completed(importInfo);

            return op;
        }
    }
}