using PlayRandom.Views.Windows;

namespace PlayRandom;

public static class TaskExtensions {

    /// <summary> Begins the task. </summary>
    /// <param name="Task"> The task. </param>
    /// <param name="Action"> The action to perform when the task completes. </param>
    /// <param name="ExceptionHandler"> The exception handler. </param>
    /// <returns> The task. </returns>
    public static void Forget( this Task Task, Action? Action = null, Action<AggregateException>? ExceptionHandler = null ) {
        TaskCompletionSource TaskCompletionSource = new();
        void ContinuationFunction( Task _ ) {
            if (Task.IsFaulted) {
                ExceptionHandler ??= MainWindow.HandleException;
                ExceptionHandler.Invoke(Task.Exception);
            } else if (Task.IsCanceled) {
                TaskCompletionSource.SetCanceled();
            } else {
                TaskCompletionSource.SetResult();
            }

            Action?.Invoke();
        }

        Task.ContinueWith(ContinuationFunction, CancellationToken.None);
    }

    /// <summary> Begins the task. </summary>
    /// <param name="Task"> The task. </param>
    /// <param name="Owner"> The owner window. If an exception occurs, the exception will be displayed in a message box with this window as its parent. </param>
    /// <param name="Action"> The optional action to perform when the task completes. </param>
    public static void Forget( this Task Task, Window Owner, Action? Action = null ) {
        void ExceptionHandler( AggregateException Exception ) => MainWindow.HandleException(Owner, Exception);
        Task.Forget(Action, ExceptionHandler);
    }

    /// <summary> Begins the task. </summary>
    /// <param name="Task"> The task. </param>
    /// <param name="Owner"> The owner dependency object. If an exception occurs, the exception will be displayed in a message box with this object's window as its parent. </param>
    /// <param name="Action"> The optional action to perform when the task completes. </param>
    public static void Forget( this Task Task, DependencyObject Owner, Action? Action = null ) {
        void ExceptionHandler( AggregateException Exception ) => MainWindow.HandleException(Window.GetWindow(Owner), Exception);
        Task.Forget(Action, ExceptionHandler);
    }

    // ContinueWith discards any exceptions making the resultant task appear to be successful. The below extensions handle exceptions properly.

    sealed class ContinueWithException : Exception {
        public ContinueWithException( Exception InnerException ) : base("An exception was thrown in a continuation task.", InnerException) { }
    }

    /// <summary> Creates a continuation task that executes asynchronously when the target task completes. </summary>
    /// <param name="Task"> The task. </param>
    /// <param name="ContinuationAction"> The action to perform when the task completes. </param>
    public static Task ContinueWithSafe( this Task Task, Action<Task> ContinuationAction ) {
        TaskCompletionSource TaskCompletionSource = new();

        void ContinuationFunction( Task T ) {
            if (T.IsFaulted) {
                TaskCompletionSource.SetException(T.Exception.InnerExceptions);
            } else if (T.IsCanceled) {
                TaskCompletionSource.SetCanceled();
            } else {
                try {
                    ContinuationAction(T);
                    TaskCompletionSource.SetResult();
                } catch (Exception Exception) {
                    TaskCompletionSource.SetException(new ContinueWithException(Exception));
                }
            }
        }

        Task.ContinueWith(ContinuationFunction);
        return TaskCompletionSource.Task;
    }

}
