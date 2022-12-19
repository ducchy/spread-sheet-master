namespace SpreadSheetMaster
{
	using System.Collections;
	using UnityEngine.Networking;

	public class SheetDownloader
	{
		private const string URI_FORMAT = "https://docs.google.com/spreadsheets/d/{0}/gviz/tq?tqx=out:csv&sheet={1}";

		public IEnumerator DownloadSheetAsync(string spreadSheetId, string sheetName, System.Action<string> onSuccess, System.Action<string> onError)
		{
			UnityWebRequest request = UnityWebRequest.Get(string.Format(URI_FORMAT, spreadSheetId, sheetName));
			yield return request.SendWebRequest();

			if (request.result == UnityWebRequest.Result.Success)
			{
				if (request.downloadHandler.text.IndexOf("https://accounts.google.com/v3/signin/") != -1)
					onError?.Invoke("サインインが要求されました。\nスプレッドシートの公開設定を変更してください。");
				else
					onSuccess?.Invoke(request.downloadHandler.text);
			}
			else
				onError?.Invoke(request.error);
		}
	}
}

