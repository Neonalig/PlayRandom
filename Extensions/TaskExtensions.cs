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

    /// <summary> Appends the given exception handler to the task. </summary>
    /// <param name="Task"> The task. </param>
    /// <param name="ExceptionHandler"> The exception handler. </param>
    /// <returns> The task. </returns>
    public static Task WithExceptionHandler( this Task Task, Action<AggregateException> ExceptionHandler ) {
        TaskCompletionSource TaskCompletionSource = new();

        void ContinuationFunction( Task _ ) {
            if (Task.IsFaulted) {
                ExceptionHandler.Invoke(Task.Exception);
            } else if (Task.IsCanceled) {
                TaskCompletionSource.SetCanceled();
            } else {
                TaskCompletionSource.SetResult();
            }
        }

        Task.ContinueWith(ContinuationFunction, CancellationToken.None);
        return TaskCompletionSource.Task;
    }

    /// <inheritdoc cref="WithExceptionHandler(Task, Action{AggregateException})"/>
    public static Func<Task> WithExceptionHandler( this Func<Task> Task, Action<AggregateException> ExceptionHandler ) {
        Task Handler() => Task.Invoke().WithExceptionHandler(ExceptionHandler);
        return Handler;
    }

    /// <summary> Appends the main window's exception handler to the task. </summary>
    /// <param name="Task"> The task. </param>
    /// <returns> The task. </returns>
    public static Task WithExceptionHandler( this Task Task ) => Task.WithExceptionHandler(MainWindow.HandleException);

    /// <inheritdoc cref="WithExceptionHandler(Task)"/>
    public static Func<Task> WithExceptionHandler( this Func<Task> Task ) => Task.WithExceptionHandler(MainWindow.HandleException);

}
