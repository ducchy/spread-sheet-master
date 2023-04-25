using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SpreadSheetMaster
{
    public class SheetDownloader
    {
        private readonly SheetUrlBuilder _sheetUrlBuilder = new();

        public AsyncOperationHandle<string> DownloadSheetAsync(string spreadSheetId, string sheetId,
            CancellationToken token)
        {
            var url = _sheetUrlBuilder.BuildExportUrl(spreadSheetId, sheetId);
            var op = new AsyncOperator<string>();
            DownloadSheetAsyncInternal(op, url, token).ContinueWith(_ => { }, token);
            return op;
        }

        public AsyncOperationHandle<string> DownloadSheetBySheetNameAsync(string spreadSheetId, string sheetName,
            CancellationToken token)
        {
            var url = _sheetUrlBuilder.BuildExportUrlBySheetName(spreadSheetId, sheetName);
            var op = new AsyncOperator<string>();
            DownloadSheetAsyncInternal(op, url, token).ContinueWith(_ => { }, token);
            return op;
        }


        public AsyncOperationHandle<string> DownloadSheetAsync(IImportableSpreadSheetMaster master,
            SheetDownloadKey sheetDownloadKey, CancellationToken token)
        {
            switch (sheetDownloadKey)
            {
                case SheetDownloadKey.SheetId:
                    return DownloadSheetAsync(master.spreadSheetId, master.sheetId, token);
                case SheetDownloadKey.SheetName:
                    return DownloadSheetBySheetNameAsync(master.spreadSheetId, master.sheetName, token);
                default:
                    var op = new AsyncOperator<string>();
                    op.Canceled(new InvalidOperationException($"Invalid SheetDownloadKey({sheetDownloadKey})"));
                    return op;
            }
        }

        private async Task DownloadSheetAsyncInternal(AsyncOperator<string> op, string url, CancellationToken token)
        {
            UnityWebRequest request;
            try
            {
                request = UnityWebRequest.Get(url);
                _ = request.SendWebRequest();
            }
            catch (Exception e)
            {
                op.Canceled(e);
                return;
            }

            while (!request.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    op.Canceled();
                    return;
                }

                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (request.downloadHandler.text.IndexOf("https://accounts.google.com/v3/signin/",
                        StringComparison.Ordinal) != -1)
                    op.Canceled(new InvalidOperationException("Required to sign in"));
                else
                    op.Completed(request.downloadHandler.text);
            }
            else
                op.Canceled(new Exception(request.error));
        }
    }
}