using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SpreadSheetMaster.Samples
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private SpreadSheetSetting _setting;

        private readonly CharacterMaster _characterMaster = new CharacterMaster();
        private SpreadSheetMasterImporter _importer;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private void Start()
        {
            var parser = new CsvParser(_setting.ignoreRowConditions);
            _importer = new SpreadSheetMasterImporter(_setting, parser);

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
            await _importer.ImportFromSpreadSheetAsync(_characterMaster, _setting.sheetDownloadKey, OnError, token);
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

            sb.AppendFormat("CharacterMaster: count={0}", _characterMaster.dataCount).AppendLine();
            _characterMaster.ForEach(character =>
            {
                sb.AppendFormat("- {0}", character.ToString()).AppendLine();
            });

            Debug.Log(sb.ToString());
        }
    }
}