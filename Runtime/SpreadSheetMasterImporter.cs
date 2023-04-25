using System;
using System.Threading.Tasks;

namespace SpreadSheetMaster
{
    using System.Threading;
    using UnityEngine;

    public class SpreadSheetMasterImporter
    {
        private readonly CsvParser _parser;
        private readonly LogLevel _logLevel;
        private readonly SheetDownloader _sheetDownloader = new();

        public SpreadSheetMasterImporter(CsvParser parser, LogLevel logLevel)
        {
            _parser = parser;
            _logLevel = logLevel;
        }

        public AsyncOperationHandle<ImportMasterInfo> ImportAsync(SpreadSheetSetting setting,
            IImportableSpreadSheetMaster master, CancellationToken token)
        {
            if (setting == null || master == null)
            {
                var op = new AsyncOperator<ImportMasterInfo>();
                op.Canceled(new InvalidOperationException());
                return op;
            }

            switch (setting.importSource)
            {
                case ImportSource.SpreadSheet:
                    return ImportFromSpreadSheetAsync(master, setting.sheetDownloadKey, token);
                case ImportSource.ResourceCsv:
                    return ImportFromResource(master, $"{setting.importResourceDirectoryPath}/{master.className}");
                default:
                    var op = new AsyncOperator<ImportMasterInfo>();
                    op.Canceled(new InvalidOperationException($"Invalid ImportSource({setting.importSource})"));
                    return op;
            }
        }

        public AsyncOperationHandle<ImportMasterInfo> ImportFromSpreadSheetAsync(IImportableSpreadSheetMaster master,
            SheetDownloadKey sheetDownloadKey, CancellationToken token)
        {
            var op = new AsyncOperator<ImportMasterInfo>();

            if (master == null)
            {
                op.Canceled(new InvalidOperationException());
                return op;
            }

            var sheetDownloadHandle = _sheetDownloader.DownloadSheetAsync(master, sheetDownloadKey, token);
            var key = sheetDownloadKey == SheetDownloadKey.SheetId ? master.sheetId : master.sheetName;
            ImportFromSpreadSheetInternalAsync(op, sheetDownloadHandle, master, key, token)
                .ContinueWith(_ => { }, token);
            return op;
        }

        private async Task ImportFromSpreadSheetInternalAsync(AsyncOperator<ImportMasterInfo> op,
            AsyncOperationHandle<string> sheetDownloadHandle, IImportableSpreadSheetMaster master,
            string key, CancellationToken token)
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
            ImportFromCsv(op, master, csv, key);
        }

        public AsyncOperationHandle<ImportMasterInfo> ImportFromResource(IImportableSpreadSheetMaster master,
            string resourcePath)
        {
            var op = new AsyncOperator<ImportMasterInfo>();

            var csvFile = Resources.Load<TextAsset>(resourcePath);
            if (csvFile == null)
            {
                op.Canceled(new InvalidOperationException($"Failed to load resource: path={resourcePath}"));
                return op;
            }

            return ImportFromCsv(op, master, csvFile.text);
        }

        private AsyncOperationHandle<ImportMasterInfo> ImportFromCsv(AsyncOperator<ImportMasterInfo> op,
            IImportableSpreadSheetMaster master, string csv, string key = "")
        {
            if (string.IsNullOrEmpty(csv))
            {
                op.Canceled(new InvalidOperationException("csv is empty"));
                return op;
            }

            var records = _parser.Parse(csv, excludeHeader: true);
            var importInfo = new ImportMasterInfo();
            importInfo.Initialize(master.className, key, records.Count, _logLevel);
            master.Import(records, importInfo);
            importInfo.ExportLog();
            op.Completed(importInfo);

            return op;
        }
    }
}