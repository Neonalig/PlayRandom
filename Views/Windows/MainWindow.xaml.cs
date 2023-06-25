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

    /// <summary> Shows a message box stating that an update is or isn't available. </summary>
    /// <param name="Release"> The release, or <see langword="null"/> if no update is available. </param>
    public static void ShowUpdateAvailable( ScannedRelease? Release ) {
        void Callback_Available() {
            MessageBox MBox = new() {
                ButtonLeftName        = "Yes",
                ButtonLeftAppearance  = ControlAppearance.Primary,
                ButtonRightName       = "No",
                ButtonRightAppearance = ControlAppearance.Secondary,
                Content               = $"A new version of PlayRandom is available ({Release.Value.Version}). Do you want to download it?",
                Title                 = "Update Available",
            };
            MBox.ButtonLeftClick += ( _, _ ) => {
                Release.Value.DownloadUri.OpenUrl();
                MBox.Close();
            };
            MBox.ButtonRightClick += ( _, _ ) => {
                MBox.Close();
            };
            MBox.ShowDialog();
        }
        void Callback_NotAvailable() {
            MessageBox MBox = new() {
                ButtonLeftName        = "OK",
                ButtonLeftAppearance  = ControlAppearance.Primary,
                Content               = "You are running the latest version of PlayRandom.",
                Title                 = "No Update Available",
            };
            MBox.ButtonLeftClick += ( _, _ ) => MBox.Close();
            MBox.ButtonRightClick += ( _, _ ) => MBox.Close();
            MBox.ShowDialog();
        }
        Application.Current.Dispatcher.Invoke(Release is not null ? Callback_Available : Callback_NotAvailable);
    }
}
