using System.Text;
using System.Threading;
using SpreadSheetMaster;
using UnityEngine;

namespace sample
{
	public class Sample : MonoBehaviour
	{
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
				sb.AppendFormat("- {0}: {1}", character.id, character.name).AppendLine();

			Debug.Log(sb.ToString());
		}
	}
}
