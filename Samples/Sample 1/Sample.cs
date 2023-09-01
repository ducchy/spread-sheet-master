using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SpreadSheetMaster.Samples
{
    /// <summary> 性別タイプ </summary>
    public enum GenderType
    {
        Man,
        Woman,
    }

    /// <summary> サンプル </summary>
    public class Sample : MonoBehaviour
    {
        private readonly CharacterMaster _characterMaster = new();
        private readonly CharacterDetailMaster _characterDetailMaster = new();
        private readonly CancellationTokenSource _cts = new();

        [SerializeField] private SpreadSheetSetting _setting;

        private SpreadSheetMasterImporter _importer;

        /// <summary> 開始 </summary>
        private async void Start()
        {
            var parser = new CsvParser(_setting.ignoreRowConditions);
            _importer = new SpreadSheetMasterImporter(parser, _setting.logLevel);

            var token = _cts.Token;
            await ImportMasterAllAsync(token);
        }

        /// <summary> 全マスタインポート </summary>
        private async UniTask ImportMasterAllAsync(CancellationToken token)
        {
            Debug.Log("[ImportMasterAllAsync] 開始");

            await ImportMasterAsync(_setting.spreadSheetId, _setting.sheetDataArray, token);

            Debug.Log("[ImportMasterAllAsync] 終了");
        }

        /// <summary> マスタインポート </summary>
        private async UniTask ImportMasterAsync(string spreadSheetId, SheetData[] sheetDataArray, CancellationToken token)
        {
            Debug.Log($"[ImportMasterAsync] ImportMasterAsync: spreadSheetId={spreadSheetId}");

            var characterSheetData = sheetDataArray.First(data => data.name == _characterMaster.sheetName);
            var characterMasterImportHandle =
                _importer.ImportFromSpreadSheetAsync(_characterMaster, spreadSheetId, characterSheetData.id, token);
            var characterDetailSheetData = sheetDataArray.First(data => data.name == _characterDetailMaster.sheetName);
            var characterDetailMasterImportHandle =
                _importer.ImportFromSpreadSheetAsync(_characterDetailMaster, spreadSheetId, characterDetailSheetData.id, token);

            while (!characterMasterImportHandle.IsDone || !characterDetailMasterImportHandle.IsDone)
                await Task.Yield();

            if (characterMasterImportHandle.Exception != null)
                Debug.LogError(characterMasterImportHandle.Exception.Message);

            if (characterDetailMasterImportHandle.Exception != null)
                Debug.LogError(characterDetailMasterImportHandle.Exception.Message);
        }
    }
}