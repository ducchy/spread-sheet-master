using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SpreadSheetMaster.Samples
{
    public enum GenderType
    {
        Man,
        Woman,
    }

    public class Sample : MonoBehaviour
    {
        [SerializeField] private SpreadSheetSetting _setting;

        private readonly CharacterMaster _characterMaster = new();
        private readonly CharacterDetailMaster _characterDetailMaster = new();
        private SpreadSheetMasterImporter _importer;

        private readonly CancellationTokenSource _cts = new();

        private void Start()
        {
            var parser = new CsvParser(_setting.ignoreRowConditions);
            _importer = new SpreadSheetMasterImporter(parser, _setting.logLevel);

            var token = _cts.Token;
            ImportMasterAllAsync(token);
        }

        private async void ImportMasterAllAsync(CancellationToken token)
        {
            Debug.Log("[インポート開始]");

            foreach (var spreadSheetData in _setting.spreadSheetDataArray)
                await ImportMasterAsync(spreadSheetData, token);

            Debug.Log("[インポート完了]");
        }

        private async Task ImportMasterAsync(SpreadSheetData spreadSheetData, CancellationToken token)
        {
            _characterMaster.OverwriteSpreadSheetId(spreadSheetData.id);
            _characterDetailMaster.OverwriteSpreadSheetId(spreadSheetData.id);

            var characterMasterImportHandle =
                _importer.ImportAsync(_setting, _characterMaster, token);
            var characterDetailMasterImportHandle =
                _importer.ImportAsync(_setting, _characterDetailMaster, token);

            while (!characterMasterImportHandle.IsDone || !characterDetailMasterImportHandle.IsDone)
                await Task.Yield();

            if (characterMasterImportHandle.Exception != null)
                Debug.LogError(characterMasterImportHandle.Exception.Message);

            if (characterDetailMasterImportHandle.Exception != null)
                Debug.LogError(characterDetailMasterImportHandle.Exception.Message);
        }
    }
}