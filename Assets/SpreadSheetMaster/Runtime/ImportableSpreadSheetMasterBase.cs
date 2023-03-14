namespace SpreadSheetMaster
{
	using System.Collections.Generic;

	public abstract class ImportableSpreadSheetMasterBase<T> : IImportableSpreadSheetMaster where T : ImportableSpreadSheetMasterDataBase, new()
	{
		public abstract string defaultSpreadSheetId { get; }
		public abstract string sheetId { get; }

		private string _overwriteSpreadSheetId = string.Empty;
		public string spreadSheetId => !string.IsNullOrEmpty(_overwriteSpreadSheetId) ? _overwriteSpreadSheetId : defaultSpreadSheetId;

		public IReadOnlyList<T> datas { get { return _datas; } }
		protected List<T> _datas = new List<T>();

		public virtual void PreImport()
		{
			_datas.Clear();
		}

		public virtual void Import(IReadOnlyList<IReadOnlyList<string>> records)
		{
			for (int i = 0; i < records.Count; i++)
			{
				T masterData = new T();
				masterData.SetData(records[i]);
				_datas.Add(masterData);
			}
		}

		public virtual void PostImport()
		{
		}

		public T GetData(int id)
		{
			if (_datas == null)
				return null;

			for (int i = 0; i < _datas.Count; i++)
			{
				T data = _datas[i];
				if (data.GetId() == id)
					return data;
			}

			return null;
		}

		public T GetDataByIndex(int index)
		{
			return IndexOutOfRange(index) ? null : _datas[index];
		}

		private bool IndexOutOfRange(int index)
		{
			return _datas == null || index < 0 || _datas.Count <= index;
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
