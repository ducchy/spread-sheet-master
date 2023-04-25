using System;

namespace SpreadSheetMaster
{
    using System.Collections.Generic;

    public abstract class ImportableSpreadSheetMasterBase<TMasterData> : IImportableSpreadSheetMaster
        where TMasterData : ImportableSpreadSheetMasterDataBase, new()
    {
        public string className => GetType().Name;
        protected abstract string defaultSpreadSheetId { get; }
        public abstract string sheetId { get; }
        public abstract string sheetName { get; }

        private string _overwriteSpreadSheetId = string.Empty;

        public string spreadSheetId => !string.IsNullOrEmpty(_overwriteSpreadSheetId)
            ? _overwriteSpreadSheetId
            : defaultSpreadSheetId;

        public IReadOnlyList<int> keys => _keys;
        protected readonly Dictionary<int, TMasterData> _dataDictionary = new();
        protected readonly List<int> _keys = new();

        public int dataCount => _keys.Count;

        public void Import(IReadOnlyList<IReadOnlyList<string>> records, ImportMasterInfo importInfo)
        {
            _dataDictionary.Clear();
            _keys.Clear();
            PreImport();
            foreach (var record in records)
            {
                var data = new TMasterData();
                data.SetData(record, importInfo);
                _dataDictionary.Add(data.GetKey(), data);
                _keys.Add(data.GetKey());
                
                importInfo.Imported(data.ToString());
            }

            PostImport();
        }

        protected virtual void PreImport()
        {
        }

        protected virtual void PostImport()
        {
        }

        public TMasterData GetData(int key)
        {
            return TryGetValue(key, out var value) ? value : null;
        }

        public TMasterData GetDataByIndex(int index)
        {
            return IndexOutOfRange(index) ? null : _dataDictionary[_keys[index]];
        }

        public bool TryGetValue(int key, out TMasterData value)
        {
            return _dataDictionary.TryGetValue(key, out value);
        }

        public TMasterData Find(Func<TMasterData, bool> condition)
        {
            if (condition == null)
                return null;

            foreach (var key in _keys)
                if (condition.Invoke(_dataDictionary[key]))
                    return _dataDictionary[key];

            return null;
        }

        public List<TMasterData> FindAll(Func<TMasterData, bool> condition)
        {
            if (condition == null)
                return null;

            var ret = new List<TMasterData>();
            foreach (var key in _keys)
                if (condition.Invoke(_dataDictionary[key]))
                    ret.Add(_dataDictionary[key]);

            return ret;
        }

        public void ForEach(Action<TMasterData> action)
        {
            if (action == null)
                return;

            foreach (var key in _keys)
                action.Invoke(_dataDictionary[key]);
        }

        public void ForEach(Func<TMasterData, bool> breakFunc)
        {
            if (breakFunc == null)
                return;

            foreach (var key in _keys)
                if (breakFunc.Invoke(_dataDictionary[key]))
                    break;
        }

        private bool IndexOutOfRange(int index)
        {
            return _keys == null || index < 0 || _keys.Count <= index;
        }

        public void OverwriteSpreadSheetId(string id)
        {
            _overwriteSpreadSheetId = id;
        }

        public void ClearOverwriteSpreadSheetId()
        {
            _overwriteSpreadSheetId = string.Empty;
        }
    }
}