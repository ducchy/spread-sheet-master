using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SpreadSheetMaster {
	/// <summary> シートダウンローダー </summary>
	public class SheetDownloader {
		#region Variables

		/// <summary> シートURLビルダー </summary>
		private readonly SheetUrlBuilder _sheetUrlBuilder = new();

		#endregion

		#region Methods

		/// <summary> シートダウンロード </summary>
		public AsyncOperationHandle<string> DownloadSheetAsync(
			string spreadSheetId,
			string sheetId,
			CancellationToken token) {
			var url = _sheetUrlBuilder.BuildExportCsvUrl(spreadSheetId, sheetId);
			var op = new AsyncOperator<string>();
			DownloadSheetAsyncInternal(op, url, token).ContinueWith(_ => { }, token);
			return op;
		}

		/// <summary> シートダウンロード </summary>
		private async Task DownloadSheetAsyncInternal(
			AsyncOperator<string> op,
			string url,
			CancellationToken token) {
			UnityWebRequest request;
			try {
				request = UnityWebRequest.Get(url);
				_ = request.SendWebRequest();
			} catch (Exception e) {
				op.Canceled(e);
				return;
			}

			while (!request.isDone) {
				if (token.IsCancellationRequested) {
					token.ThrowIfCancellationRequested();
					op.Canceled();
					return;
				}

				await Task.Yield();
			}

			if (request.result == UnityWebRequest.Result.Success) {
				if (request.downloadHandler.text.IndexOf("https://accounts.google.com/v3/signin/",
					    StringComparison.Ordinal) != -1) {
					op.Canceled(new InvalidOperationException("Required to sign in"));
				} else {
					op.Completed(request.downloadHandler.text);
				}
			} else {
				op.Canceled(new Exception(request.error));
			}
		}

		#endregion
	}
}