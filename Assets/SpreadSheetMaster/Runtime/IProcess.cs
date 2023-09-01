using System;
using System.Collections;

namespace SpreadSheetMaster
{
    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public interface IProcess<T> : IEnumerator
    {
        /// <summary> 結果 </summary>
        T Result { get; }

        /// <summary> 完了しているか </summary>
        bool IsDone { get; }

        /// <summary> エラー </summary>
        Exception Exception { get; }
    }
}