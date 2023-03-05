using System.Text;
using System.Threading;
using UnityEngine;

namespace SpreadSheetMaster.Samples
{
	public class Sample : MonoBehaviour
	{
		private const string OVERWRITE_SPREAD_SHEET_ID = "1cjSUtkl0frO8OjBKWNPfBYMGpPzLnTAz3ZGiwK6pNo8";

		private readonly CharacterMaster _characterMaster = new CharacterMaster();
		private readonly SpreadSheetMasterImporter _importer = new SpreadSheetMasterImporter();

		private readonly CancellationTokenSource _cts = new CancellationTokenSource();
		private CancellationToken ct => _cts.Token;


		private void Start()
		{
			ImportMasterAllAsync();
		}

		private async void ImportMasterAllAsync()
		{
			Debug.Log("[インポート開始]");

			await _importer.ImportFromSpreadSheetAsync(_characterMaster, OnError, ct);
			OnCompletedImport();

			_characterMaster.OverwriteSpreadSheetId(OVERWRITE_SPREAD_SHEET_ID);

			await _importer.ImportFromSpreadSheetAsync(_characterMaster, OnError, ct);
			OnCompletedImport();
		}

		private void OnError(string error)
		{
			Debug.LogError(error);
		}

		private void OnCompletedImport()
		{
			var sb = new StringBuilder();
			sb.Append("[インポート完了]").AppendLine();

			var characters = _characterMaster.datas;
			sb.AppendFormat("CharacterMaster: count={0}", characters.Count).AppendLine();
			foreach (var character in characters)
				sb.AppendFormat("- {0}", character.ToString()).AppendLine();

			Debug.Log(sb.ToString());
		}
	}
}
