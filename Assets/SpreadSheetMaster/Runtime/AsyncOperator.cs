using System;

namespace SpreadSheetMaster {
	/// <summary> 非同期処理ハンドル用オペレーター </summary>
	public class AsyncOperator<T> {
		#region Constants

		/// <summary> 完了通知イベント </summary>
		public event Action<T> OnCompletedEvent;

		/// <summary> キャンセル通知イベント </summary>
		public event Action<Exception> OnCanceledEvent;

		#endregion

		#region Variables

		/// <summary> 完了しているか </summary>
		public bool IsDone => IsCompleted || Exception != null;

		/// <summary> 結果 </summary>
		public T Result { get; private set; }

		/// <summary> 正常終了か </summary>
		public bool IsCompleted { get; private set; }

		/// <summary> エラー終了か </summary>
		public Exception Exception { get; private set; }

		#endregion

		#region Methods

		/// <summary> ハンドルへの暗黙型変換 </summary>
		public static implicit operator AsyncOperationHandle<T>(AsyncOperator<T> source) {
			return source.GetHandle();
		}

		/// <summary> ハンドル取得 </summary>
		public AsyncOperationHandle<T> GetHandle() {
			return new AsyncOperationHandle<T>(this);
		}

		/// <summary> 完了時処理 </summary>
		public void Completed(T result) {
			if (IsDone) {
				throw new InvalidOperationException("Duplicate completion action.");
			}

			Result = result;
			IsCompleted = true;
			OnCompletedEvent?.Invoke(result);
			OnCompletedEvent = null;
			OnCanceledEvent = null;
		}

		/// <summary> キャンセル時処理 </summary>
		/// <param name="exception">キャンセル原因</param>
		public void Canceled(Exception exception = null) {
			if (IsDone) {
				throw new InvalidOperationException("Duplicate cancel action.");
			}

			exception ??= new OperationCanceledException("Canceled operation");

			Exception = exception;
			OnCanceledEvent?.Invoke(exception);
			OnCompletedEvent = null;
			OnCanceledEvent = null;
		}

		#endregion
	}
}