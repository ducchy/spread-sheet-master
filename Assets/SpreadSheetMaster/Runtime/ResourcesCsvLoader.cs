using System;
using System.Threading;
using UnityEngine;

namespace SpreadSheetMaster {
	/// <summary> リソースCSVローダー </summary>
	public class ResourcesCsvLoader : ICsvLoader {
		#region Variables

		private readonly string _resourcePath;

		#endregion

		#region Methods

		public ResourcesCsvLoader(string resourcePath) {
			_resourcePath = resourcePath;
		}

		/// <summary> ロード </summary>
		public AsyncOperationHandle<string> LoadAsync(CancellationToken token) {
			var op = new AsyncOperator<string>();

			var csvFile = Resources.Load<TextAsset>(_resourcePath);
			if (csvFile == null) {
				op.Canceled(new InvalidOperationException($"Failed to load resource: path={_resourcePath}"));
				return op;
			}

			op.Completed(csvFile.text);
			return op;
		}

		#endregion
	}
}