using System.Threading;

namespace SpreadSheetMaster
{
    /// <summary> CSVローダー </summary>
    public interface ICsvLoader
    {
        /// <summary> CSVをロード </summary>
        AsyncOperationHandle<string> LoadAsync(CancellationToken token);
    }
}