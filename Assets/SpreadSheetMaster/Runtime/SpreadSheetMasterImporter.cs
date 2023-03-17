namespace SpreadSheetMaster
{
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class SpreadSheetMasterImporter
    {
        private readonly SpreadSheetSetting _setting;
        private readonly CsvParser _parser;
        private readonly SheetDownloader _sheetDownloader = new SheetDownloader();

        public SpreadSheetMasterImporter(SpreadSheetSetting setting, CsvParser parser)
        {
            _setting = setting;
            _parser = parser;
        }

        public async Task ImportFromSpreadSheetAsync(IImportableSpreadSheetMaster master,
            SheetDownloadKey sheetDownloadKey, System.Action<string> onError,
            CancellationToken token)
        {
            var csv = string.Empty;
            await _sheetDownloader.DownloadSheetAsync(master, sheetDownloadKey, (str) => csv = str, onError, token);

            ImportFromCsv(master, csv);
        }

        public void ImportFromResource(IImportableSpreadSheetMaster master, string resourcePath)
        {
            var csvFile = Resources.Load<TextAsset>(resourcePath);
            if (csvFile == null)
            {
                Debug.LogFormat("csvファイルのリソースロード失敗: resourcePath={0}", resourcePath);
                return;
            }

            ImportFromCsv(master, csvFile.text);
        }

        public void ImportFromCsv(IImportableSpreadSheetMaster master, string csv)
        {
            if (csv == string.Empty)
                return;

            master.Import(_parser.Parse(csv, excludeHeader: true));
        }
    }
}