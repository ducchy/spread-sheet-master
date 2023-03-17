using System;

namespace SpreadSheetMaster
{
    using System.Collections.Generic;

    public abstract class ImportableSpreadSheetMasterBase<T> : IImportableSpreadSheetMaster
        where T : ImportableSpreadSheetMasterDataBase, new()
    {
        protected abstract string defaultSpreadSheetId { get; }
        public abstract string sheetId { get; }
        public abstract string sheetName { get; }

        private string _overwriteSpreadSheetId = string.Empty;

        public string spreadSheetId => !string.IsNullOrEmpty(_overwriteSpreadSheetId)
            ? _overwriteSpreadSheetId
            : defaultSpreadSheetId;

        public IReadOnlyList<T> dataList => _dataList;
        protected List<T> _dataList = new List<T>();
        
        public int dataCount => _dataList?.Count ?? 0;

        public T this[int i] => GetData(i);

        public void Import(IReadOnlyList<IReadOnlyList<string>> records)
        {
            _dataList.Clear();
            PreImport();
            foreach (var record in records)
            {
                var data = new T();
                data.SetData(record);
                _dataList.Add(data);
            }
            PostImport();
        }

        protected virtual void PreImport()
        {
        }

        protected virtual void PostImport()
        {
        }

        public T GetData(int id)
        {
            if (_dataList == null)
                return null;

            foreach (var data in _dataList)
                if (data.GetId() == id)
                    return data;

            return null;
        }

        public T GetDataByIndex(int index)
        {
            return IndexOutOfRange(index) ? null : _dataList[index];
        }

        public bool TryGetValue(int id, out T value)
        {
            value = GetData(id);
            return value != null;
        }

        public T Find(Func<T, bool> condition)
        {
            if (condition == null)
                return null;
            
            foreach (var data in _dataList)
                if (condition.Invoke(data))
                    return data;

            return null;
        }

        public List<T> FindAll(Func<T, bool> condition)
        {
            if (condition == null)
                return null;

            var ret = new List<T>();
            foreach (var data in _dataList)
                if (condition.Invoke(data))
                    ret.Add(data);

            return ret;
        }

        private bool IndexOutOfRange(int index)
        {
            return _dataList == null || index < 0 || _dataList.Count <= index;
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