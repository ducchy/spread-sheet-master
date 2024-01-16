using System;
using System.Collections.Generic;

namespace SpreadSheetMaster {
	/// <summary> インポート可能なスプレッドシートマスタ </summary>
	public abstract class ImportableSpreadSheetMasterBase<TMasterData> : IImportableSpreadSheetMaster
		where TMasterData : ImportableSpreadSheetMasterDataBase, new() {
		#region Variables

		public string ClassName => GetType().Name;
		public IReadOnlyList<int> Keys => _keys;
		public int DataCount => _keys.Count;

		public abstract string SheetName { get; }
		protected readonly Dictionary<int, TMasterData> _dataDictionary = new();
		protected readonly List<int> _keys = new();

		#endregion

		#region Methods

		/// <summary> インポート </summary>
		public void Import(IReadOnlyList<IReadOnlyList<string>> records, ImportMasterLogBuilder importLogBuilder) {
			_dataDictionary.Clear();
			_keys.Clear();
			PreImport();
			foreach (var record in records) {
				var data = new TMasterData();
				data.SetData(record, importLogBuilder);

#if SSM_LOG
				if (_dataDictionary.ContainsKey(data.GetKey())) {
					importLogBuilder.DuplicateKey(data.GetKey());
				} else {
					_dataDictionary.Add(data.GetKey(), data);
				}
#else
				_dataDictionary.Add(data.GetKey(), data);
#endif
				_keys.Add(data.GetKey());

				importLogBuilder.ImportedData(data.ToString());
			}

			PostImport();
		}

		/// <summary> データ取得 </summary>
		public TMasterData GetData(int key) {
			return TryGetValue(key, out var value) ? value : null;
		}

		/// <summary> インデックスに対応するデータ取得 </summary>
		public TMasterData GetDataByIndex(int index) {
			return IndexOutOfRange(index) ? null : _dataDictionary[_keys[index]];
		}

		/// <summary> キーに対応する値取得 </summary>
		public bool TryGetValue(int key, out TMasterData value) {
			return _dataDictionary.TryGetValue(key, out value);
		}

		/// <summary> データ検索 </summary>
		public TMasterData Find(Func<TMasterData, bool> condition) {
			if (condition == null) {
				return null;
			}

			foreach (var key in _keys) {
				if (condition.Invoke(_dataDictionary[key])) {
					return _dataDictionary[key];
				}
			}

			return null;
		}

		/// <summary> データ検索 </summary>
		public List<TMasterData> FindAll(Func<TMasterData, bool> condition) {
			if (condition == null) {
				return null;
			}

			var ret = new List<TMasterData>();
			foreach (var key in _keys) {
				if (condition.Invoke(_dataDictionary[key])) {
					ret.Add(_dataDictionary[key]);
				}
			}

			return ret;
		}

		/// <summary> 全データに対する処理 </summary>
		public void ForEach(Action<TMasterData> action) {
			if (action == null) {
				return;
			}

			foreach (var key in _keys) {
				action.Invoke(_dataDictionary[key]);
			}
		}

		/// <summary> 全データに対する処理 </summary>
		public void ForEach(Func<TMasterData, bool> breakFunc) {
			if (breakFunc == null) {
				return;
			}

			foreach (var key in _keys) {
				if (breakFunc.Invoke(_dataDictionary[key])) {
					break;
				}
			}
		}

		/// <summary> インポート前処理 </summary>
		protected virtual void PreImport() { }

		/// <summary> インポート後処理 </summary>
		protected virtual void PostImport() { }

		/// <summary> インデックスが範囲外か </summary>
		private bool IndexOutOfRange(int index) {
			return _keys == null || index < 0 || _keys.Count <= index;
		}

		#endregion

		public TMasterData this[int key] => GetData(key);
	}
}