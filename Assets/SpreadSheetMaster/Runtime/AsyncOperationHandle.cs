using System;
using System.Collections;

namespace SpreadSheetMaster
{
    /// <summary>
    /// 一連の処理を表すインターフェース
    /// </summary>
    public readonly struct AsyncOperationHandle<T> : IProcess<T>
    {
        private readonly AsyncOperator<T> _asyncOperator;

        // 結果
        public T Result => _asyncOperator != null ? _asyncOperator.Result : default;

        // 完了しているか
        public bool IsDone => _asyncOperator == null || _asyncOperator.IsDone;

        // キャンセル時のエラー
        public Exception Exception => _asyncOperator?.Exception;

        /// <inheritdoc/>
        object IEnumerator.Current => null;

        /// <summary>
        /// 通知の購読
        /// </summary>
        /// <param name="onCompleted">完了時通知</param>
        /// <param name="onCanceled">キャンセル時通知</param>
        public void ListenTo(Action<T> onCompleted, Action<Exception> onCanceled = null)
        {
            if (_asyncOperator == null || _asyncOperator.IsDone)
            {
                return;
            }

            if (onCompleted != null)
            {
                _asyncOperator.OnCompletedEvent += onCompleted;
            }

            if (onCanceled != null)
            {
                _asyncOperator.OnCanceledEvent += onCanceled;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="asyncOperator">非同期通知用インスタンス</param>
        internal AsyncOperationHandle(AsyncOperator<T> asyncOperator)
        {
            _asyncOperator = asyncOperator;
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }

        /// <inheritdoc/>
        void IEnumerator.Reset()
        {
            throw new NotImplementedException();
        }
    }
}