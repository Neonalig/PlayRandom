// #define FRAME_LOG

using PlayRandom.Models;
using PlayRandom.ViewModels;

namespace PlayRandom.Views.Windows;

/// <summary> Interaction logic for MainWindow.xaml </summary>
public partial class MainWindow {
    /// <summary> Initialises a new instance of the <see cref="MainWindow"/> class. </summary>
    public MainWindow() {
        InitializeComponent();

        // Create instances of the model and view model
        MainViewModel ViewModel = new();

        // Set the view model as the data context
        DataContext = ViewModel;

        // Listen for operating system theme changes
        Loaded += OnLoaded;
        void OnLoaded( object? Sender, RoutedEventArgs E ) {
            Watcher.Watch(
                this,                // Window class
                BackgroundType.Mica, // Background type
                true                 // Whether to change accents automatically
            );
        }

        // Detect UI thread hangs (in background thread)
        #if DEBUG
        UIThreadHangDetector.Start();
        Application.Current.Exit += ( _, _ ) => UIThreadHangDetector.Stop();
        #endif
    }

    void MainWindow_OnClosing( object? Sender, CancelEventArgs E ) {
        // Prompt to save if there are any pending changes
        if (!Settings.HasUnsavedChanges) { return; }
        MessageBox MBox = new() {
            ButtonLeftName        = "Yes",
            ButtonLeftAppearance  = ControlAppearance.Primary,
            ButtonRightName       = "No",
            ButtonRightAppearance = ControlAppearance.Secondary,
            Content               = "You have unsaved changes. Do you want to save them?",
            Title                 = "Unsaved Changes",
        };
        MBox.ButtonLeftClick += ( _, _ ) => {
            Settings.Save();
            MBox.Close();
        };
        MBox.ButtonRightClick += ( _, _ ) => {
            MBox.Close();
        };
        MBox.ShowDialog();
    }

    /// <summary> Catches an exception and displays it in a message box. </summary>
    /// <param name="Exception"> The exception. </param>
    public static void HandleException( Exception Exception ) {
        static void GetExceptionMessage( StringBuilder Builder, Exception Exception ) {
            Builder.AppendLine(Exception.Message);
            Exception? InnerException = Exception.InnerException;
            while (InnerException != null) {
                Builder.AppendLine(InnerException.Message);
                InnerException = InnerException.InnerException;
            }
        }
        HandleException(Exception, GetExceptionMessage);
    }

    /// <inheritdoc cref="HandleException(Exception)"/>
    public static void HandleException( AggregateException Exception ) {
        static void GetExceptionMessage( StringBuilder Builder, AggregateException Exception ) {
            foreach (Exception InnerException in Exception.InnerExceptions) {
                Builder.AppendLine(InnerException.Message);
            }
        }
        HandleException(Exception, GetExceptionMessage);
    }

    static void HandleException<TException>( TException Exception, Action<StringBuilder, TException> AppendExceptionMessage ) where TException : Exception {
        if (Debugger.IsAttached) {
            Trace.TraceError(Exception.ToString());
        }
        StringBuilder Message = new();
        AppendExceptionMessage(Message, Exception);
        ShowExceptionMessage(Message.ToString(), Exception.ToString());
    }

    static void ShowExceptionMessage( string Message, string Details ) {
        MessageBox MBox = new() {
            ButtonLeftName        = "OK",
            ButtonLeftAppearance  = ControlAppearance.Primary,
            ButtonRightName       = "Details",
            ButtonRightAppearance = ControlAppearance.Secondary,
            Content               = Message,
            Title                 = "Error",
        };
        MBox.ButtonLeftClick += ( _, _ ) => MBox.Close();
        MBox.ButtonRightClick += ( _, _ ) => {
            MBox.Content         = Details;
            MBox.ButtonRightName = "Copy";
            MBox.ButtonRightClick += ( _, _ ) => {
                Clipboard.SetText(Details);
                MBox.Close();
            };
        };
        MBox.ShowDialog();
    }

    public static void ShowUpdateAvailable( ScannedRelease Release ) {
        void Callback() {
            MessageBox MBox = new() {
                ButtonLeftName        = "Yes",
                ButtonLeftAppearance  = ControlAppearance.Primary,
                ButtonRightName       = "No",
                ButtonRightAppearance = ControlAppearance.Secondary,
                Content               = $"A new version of PlayRandom is available ({Release.Version}). Do you want to download it?",
                Title                 = "Update Available",
            };
            MBox.ButtonLeftClick += ( _, _ ) => {
                Process.Start(new ProcessStartInfo(Release.DownloadUri.ToString()) {
                    UseShellExecute = true
                });
                MBox.Close();
            };
            MBox.ButtonRightClick += ( _, _ ) => {
                MBox.Close();
            };
            MBox.ShowDialog();
        }
        Application.Current.Dispatcher.Invoke(Callback);
    }

    #if DEBUG
    static class UIThreadHangDetector {
        static readonly CancellationTokenSource _CTS = new();
        static readonly SynchronizationContext  _Context;

        static UIThreadHangDetector() {
            #if !DEBUG
            throw new InvalidOperationException("This method should only be called in debug mode");
            #endif

            // Getting the synchronisation context of the UI thread
            _Context = SynchronizationContext.Current ?? throw new InvalidOperationException("No synchronisation context");
            Task.Run(Poll, _CTS.Token);
        }

        /// <summary> Starts the UI thread hang detector. </summary>
        public static void Start() => _CTS.Token.ThrowIfCancellationRequested();

        /// <summary> Stops the UI thread hang detector. </summary>
        public static void Stop() => _CTS.Cancel();

        static async Task Poll() {
            // const float DesiredFrameRate = 60; // Desired frame rate for debug mode
            // TimeSpan Timeout = TimeSpan.FromMilliseconds(1000 / DesiredFrameRate);
            TimeSpan Timeout = TimeSpan.FromSeconds(1);

            Stopwatch Stopwatch = new();
            for (;;) {
                if (_CTS.IsCancellationRequested) {
                    // Debug.WriteLine("UI thread hang detector stopped.");
                    return;
                }

                TaskCompletionSource TCS = new();

                _Context.Post(SendOrPostCallback, null);
                void SendOrPostCallback( object? State ) {
                    try {
                        // Schedule a no-op on the UI thread
                        TCS.SetResult();
                    } catch (Exception E) {
                        TCS.SetException(E);
                    }
                }

                Stopwatch.Restart();
                await TCS.Task;

                // Print the elapsed time for each frame
                #if FRAME_LOG
                long Ms = Stopwatch.ElapsedMilliseconds;
                Debug.WriteLine(
                    Ms > 0
                        ? $"UI responded in {Stopwatch.ElapsedMilliseconds}ms"
                        : $"UI responded in <1ms ({Stopwatch.ElapsedTicks} ticks)"
                );
                #endif

                if (Stopwatch.Elapsed > Timeout) {
                    // UI thread did not respond within timeout, consider this a hang
                    Debug.WriteLine("UI thread hang detected.");
                }

                await Task.Delay(Timeout); // Check every 'timeout' period
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
    #endif
}
