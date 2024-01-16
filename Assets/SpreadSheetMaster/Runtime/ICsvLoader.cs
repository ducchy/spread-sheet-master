using System.Threading;

namespace SpreadSheetMaster {
	/// <summary> CSVローダー </summary>
	public interface ICsvLoader {
		#region Methods

		/// <summary> CSVをロード </summary>
		AsyncOperationHandle<string> LoadAsync(CancellationToken token);

		#endregion
	}
}