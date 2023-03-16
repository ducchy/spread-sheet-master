using System;

namespace SpreadSheetMaster
{
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    public class SheetDownloader
    {
        private readonly SheetUrlBuilder _sheetUrlBuilder = new SheetUrlBuilder();

        public async Task DownloadSheetAsync(string spreadSheetId, string sheetId, Action<string> onSuccess,
            Action<string> onError, CancellationToken token)
        {
            var url = _sheetUrlBuilder.BuildExportUrl(spreadSheetId, sheetId);
            await DownloadSheetAsyncInternal(url, onSuccess, onError, token);
        }

        public async Task DownloadSheetBySheetNameAsync(string spreadSheetId, string sheetName, Action<string> onSuccess,
            Action<string> onError, CancellationToken token)
        {
            var url = _sheetUrlBuilder.BuildExportUrlBySheetName(spreadSheetId, sheetName);
            await DownloadSheetAsyncInternal(url, onSuccess, onError, token);
        }

        private async Task DownloadSheetAsyncInternal(string url, Action<string> onSuccess,
            Action<string> onError, CancellationToken token)
        {
            var request = UnityWebRequest.Get(url);
            _ = request.SendWebRequest();

            while (!request.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    Debug.Log("Task {0} cancelled");
                    token.ThrowIfCancellationRequested();
                }

                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (request.downloadHandler.text.IndexOf("https://accounts.google.com/v3/signin/",
                        StringComparison.Ordinal) != -1)
                    onError?.Invoke("サインインが要求されました。\nスプレッドシートの公開設定を変更してください。");
                else
                    onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
                onError?.Invoke(request.error);
        }
    }
}