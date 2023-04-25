using System;

namespace SpreadSheetMaster
{
    /// <summary>
    /// 非同期処理ハンドル用オペレーター
    /// </summary>
    public class AsyncOperator<T>
    {
        // 結果
        public T Result { get; private set; }

        // 正常終了か
        public bool IsCompleted { get; private set; }

        // エラー終了か
        public Exception Exception { get; private set; }

        // 完了しているか
        public bool IsDone => IsCompleted || Exception != null;

        // 完了通知イベント
        public event Action<T> OnCompletedEvent;

        // キャンセル通知イベント
        public event Action<Exception> OnCanceledEvent;

        /// <summary>
        /// ハンドルへの暗黙型変換
        /// </summary>
        public static implicit operator AsyncOperationHandle<T>(AsyncOperator<T> source)
        {
            return source.GetHandle();
        }

        /// <summary>
        /// ハンドルの取得
        /// </summary>
        public AsyncOperationHandle<T> GetHandle()
        {
            return new AsyncOperationHandle<T>(this);
        }

        /// <summary>
        /// 完了時に呼び出す処理
        /// </summary>
        public void Completed(T result)
        {
            if (IsDone)
                throw new InvalidOperationException("Duplicate completion action.");

            Result = result;
            IsCompleted = true;
            OnCompletedEvent?.Invoke(result);
            OnCompletedEvent = null;
            OnCanceledEvent = null;
        }

        /// <summary>
        /// キャンセル時に呼び出す処理
        /// </summary>
        /// <param name="exception">キャンセル原因</param>
        public void Canceled(Exception exception = null)
        {
            if (IsDone)
                throw new InvalidOperationException("Duplicate cancel action.");

            exception ??= new OperationCanceledException("Canceled operation");

            Exception = exception;
            OnCanceledEvent?.Invoke(exception);
            OnCompletedEvent = null;
            OnCanceledEvent = null;
        }
    }
}