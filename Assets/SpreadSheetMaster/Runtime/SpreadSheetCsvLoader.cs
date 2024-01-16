using System.Threading;
using System.Threading.Tasks;

namespace SpreadSheetMaster {
	/// <summary> スプレッドシートCSVローダー </summary>
	public class SpreadSheetCsvLoader : ICsvLoader {
		#region Variables

		private readonly string _spreadSheetId;
		private readonly string _sheetId;

		private readonly SheetDownloader _sheetDownloader;

		#endregion

		#region Methods

		/// <summary> コンストラクタ </summary>
		public SpreadSheetCsvLoader(
			string spreadSheetId,
			string sheetId) {
			_spreadSheetId = spreadSheetId;
			_sheetId = sheetId;

			_sheetDownloader = new SheetDownloader();
		}

		/// <summary> ロード </summary>
		AsyncOperationHandle<string> ICsvLoader.LoadAsync(CancellationToken token) {
			var op = new AsyncOperator<string>();

			var sheetDownloadHandle = _sheetDownloader.DownloadSheetAsync(_spreadSheetId, _sheetId, token);
			LoadAsyncInternal(op, sheetDownloadHandle, token)
				.ContinueWith(_ => { }, token);

			return op;
		}

		/// <summary> ロード </summary>
		private async Task LoadAsyncInternal(
			AsyncOperator<string> op,
			AsyncOperationHandle<string> sheetDownloadHandle,
			CancellationToken token) {
			while (!sheetDownloadHandle.IsDone) {
				if (token.IsCancellationRequested) {
					token.ThrowIfCancellationRequested();
					op.Canceled();
					return;
				}

				await Task.Yield();
			}

			if (sheetDownloadHandle.Exception != null) {
				op.Canceled(sheetDownloadHandle.Exception);
				return;
			}

			var csv = sheetDownloadHandle.Result;
			op.Completed(csv);
		}

		#endregion
	}
}