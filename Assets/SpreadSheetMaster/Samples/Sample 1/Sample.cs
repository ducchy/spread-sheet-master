using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SpreadSheetMaster.Samples.MyEnums {
	/// <summary> 性別タイプ </summary>
	public enum GenderType {
		Man,
		Woman,
	}
}

namespace SpreadSheetMaster.Samples {
	/// <summary> サンプル </summary>
	public class Sample : MonoBehaviour {
		#region Serialize Fields

		[SerializeField] private SpreadSheetSetting _setting;

		#endregion

		#region Variables

		private readonly CharacterMaster _characterMaster = new();
		private readonly CharacterDetailMaster _characterDetailMaster = new();
		private readonly CancellationTokenSource _cts = new();

		private SpreadSheetMasterImporter _importer;

		#endregion

		#region Unity Event Functions

		/// <summary> 開始 </summary>
		private async void Start() {
			var parser = new CsvParser(_setting.IgnoreRowConditions);
			_importer = new SpreadSheetMasterImporter(parser, _setting.LogLevel);

			var token = _cts.Token;
			await ImportMasterAllAsync(token);
		}

		#endregion

		#region Methods

		/// <summary> 全マスタインポート </summary>
		private async UniTask ImportMasterAllAsync(CancellationToken token) {
			Debug.Log("[ImportMasterAllAsync] 開始");

			await ImportMasterAsync(_setting.SpreadSheetId, _setting.SheetDataArray, token);

			Debug.Log("[ImportMasterAllAsync] 終了");
		}

		/// <summary> マスタインポート </summary>
		private async UniTask ImportMasterAsync(string spreadSheetId, SheetData[] sheetDataArray, CancellationToken token) {
			Debug.Log($"[ImportMasterAsync] ImportMasterAsync: spreadSheetId={spreadSheetId}");

			var characterSheetData = sheetDataArray.First(data => data.Name == _characterMaster.SheetName);
			ICsvLoader characterCsvLoader = new SpreadSheetCsvLoader(spreadSheetId, characterSheetData.Id);
			var characterCsvHandle = characterCsvLoader.LoadAsync(token);
			await characterCsvHandle;
			var characterCsv = characterCsvHandle.Result;
			var characterMasterImportHandle = _importer.ImportAsync(_characterMaster, characterCsv);

			var characterDetailSheetData = sheetDataArray.First(data => data.Name == _characterDetailMaster.SheetName);
			ICsvLoader characterDetailCsvLoader = new SpreadSheetCsvLoader(spreadSheetId, characterDetailSheetData.Id);
			var characterDetailCsvHandle = characterDetailCsvLoader.LoadAsync(token);
			await characterDetailCsvHandle;
			var characterDetailCsv = characterDetailCsvHandle.Result;
			var characterDetailMasterImportHandle = _importer.ImportAsync(_characterDetailMaster, characterDetailCsv);

			while (!characterMasterImportHandle.IsDone || !characterDetailMasterImportHandle.IsDone) {
				await Task.Yield();
			}

			if (characterMasterImportHandle.Exception != null) {
				Debug.LogError(characterMasterImportHandle.Exception.Message);
			}

			if (characterDetailMasterImportHandle.Exception != null) {
				Debug.LogError(characterDetailMasterImportHandle.Exception.Message);
			}
		}

		#endregion
	}
}