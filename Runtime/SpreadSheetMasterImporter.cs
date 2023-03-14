namespace SpreadSheetMaster
{
	using System.Threading;
	using System.Threading.Tasks;
	using UnityEngine;

	public class SpreadSheetMasterImporter
	{
		private readonly SpreadSheetSetting _setting;
		private readonly CsvParser _parser;

		public SpreadSheetMasterImporter(SpreadSheetSetting setting)
		{
			_setting = setting;
			_parser = new CsvParser(_setting != null ? _setting.ignoreRowConditions : null);
		}
		
		public async Task ImportFromSpreadSheetAsync(IImportableSpreadSheetMaster master, System.Action<string> onError, CancellationToken ct)
		{
			string csv = string.Empty;

			SheetDownloader downloader = new SheetDownloader();
			await downloader.DownloadSheetAsync(master.spreadSheetId, master.sheetId, (str) => csv = str, onError, ct);

			ImportFromCsv(master, csv);
		}

		public void ImportFromResource(IImportableSpreadSheetMaster master, string resourcePath)
		{
			TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
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

			master.PreImport();
			master.Import(_parser.Perse(csv, excludeHeader: true));
			master.PostImport();
		}
	}
}
