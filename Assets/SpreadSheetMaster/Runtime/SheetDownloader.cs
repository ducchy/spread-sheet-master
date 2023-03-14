using System;

namespace SpreadSheetMaster
{
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    public class SheetDownloader
    {
        private const string URI_FORMAT = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";

        // ReSharper disable Unity.PerformanceAnalysis
        public async Task DownloadSheetAsync(string spreadSheetId, string sheetId, Action<string> onSuccess,
            System.Action<string> onError, CancellationToken ct)
        {
            var request = UnityWebRequest.Get(string.Format(URI_FORMAT, spreadSheetId, sheetId));
            _ = request.SendWebRequest();

            while (!request.isDone)
            {
                if (ct.IsCancellationRequested)
                {
                    Debug.Log("Task {0} cancelled");
                    ct.ThrowIfCancellationRequested();
                }

                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (request.downloadHandler.text.IndexOf("https://accounts.google.com/v3/signin/", StringComparison.Ordinal) != -1)
                    onError?.Invoke("サインインが要求されました。\nスプレッドシートの公開設定を変更してください。");
                else
                    onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
                onError?.Invoke(request.error);
        }
    }
}