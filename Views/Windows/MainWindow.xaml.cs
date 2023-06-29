using PlayRandom.Models;
using PlayRandom.ViewModels;

namespace PlayRandom.Views.Windows;

/// <summary> Interaction logic for MainWindow.xaml </summary>
public partial class MainWindow {
    /// <summary> Initialises a new instance of the <see cref="MainWindow"/> class. </summary>
    public MainWindow() {
        InitializeComponent();

        // Listen for operating system theme changes
        Loaded += OnLoaded;
        void OnLoaded( object? Sender, RoutedEventArgs E ) {
            Watcher.Watch(
                this,                // Window class
                BackgroundType.Mica, // Background type
                true                 // Whether to change accents automatically
            );
        }

        Topmost = AlwaysOnTopMenuItemChecked = ViewModel.AlwaysOnTop;

        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    MenuItem AlwaysOnTopMenuItem {
        get {
            foreach (object? Item in NotifyIconMenu.Items) {
                if (Item is MenuItem { Tag: "always-on-top" } MenuItem) {
                    return MenuItem;
                }
            }
            throw new InvalidOperationException("Could not find the always-on-top menu item.");
        }
    }

    bool AlwaysOnTopMenuItemChecked {
        get => AlwaysOnTopMenuItem.SymbolIcon == SymbolRegular.Checkmark28;
        set => AlwaysOnTopMenuItem.SymbolIcon = value ? SymbolRegular.Checkmark28 : SymbolRegular.Empty;
    }

    PipeServer? _PipeServer;
    Task?       _PipeServerTask;

    public readonly struct PipeServer {
        public readonly NamedPipeServerStream   Stream;
        public readonly CancellationTokenSource Cancellation;

        public PipeServer( NamedPipeServerStream Stream, CancellationTokenSource Cancellation ) {
            this.Stream       = Stream;
            this.Cancellation = Cancellation;
        }

        public void Deconstruct( out NamedPipeServerStream Stream, out CancellationTokenSource Cancellation ) {
            Stream       = this.Stream;
            Cancellation = this.Cancellation;
        }
    }

    void StartNamedPipeServer() {
        CancellationTokenSource Cancellation = new();
        async Task RunServer() {
            await using NamedPipeServerStream Server = new(Messages.LocalPipe, PipeDirection.In);
            while (true) {
                if (Cancellation.IsCancellationRequested) {
                    return;
                }

                await Server.WaitForConnectionAsync(Cancellation.Token);

                using StreamReader Reader  = new(Server);
                string?            Message = await Reader.ReadLineAsync(Cancellation.Token);

                switch (Message) {
                    case Messages.BringToFront:
                        void BringToFront() {
                            Show();
                            WindowState = WindowState.Normal;
                            Activate();
                        }
                        await Application.Current.Dispatcher.InvokeAsync(BringToFront);
                        break;
                }

                Server.Disconnect();
            }
        }
        _PipeServerTask = Task.Run(RunServer, Cancellation.Token);
    }


    /// <summary> Gets the view model. </summary>
    MainViewModel ViewModel => (MainViewModel)DataContext;

    void OnViewModelPropertyChanged( object? Sender, PropertyChangedEventArgs E ) {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (E.PropertyName) {
            case nameof(MainViewModel.AlwaysOnTop):
                Topmost = AlwaysOnTopMenuItemChecked = ViewModel.AlwaysOnTop;
                break;
        }
    }

    #region Overrides of Window

    /// <inheritdoc />
    protected override void OnStateChanged( EventArgs E ) {
        base.OnStateChanged(E);

        if (WindowState == WindowState.Minimized && Settings.MinimiseToTray) {
            // E.Handled = true; // Doesn't exist. :(
            WindowState = WindowState.Normal;
            MinimiseToTray();
        }
    }

    /// <inheritdoc />
    protected override void OnClosing( CancelEventArgs E ) {
        base.OnClosing(E);

        if (!E.Cancel && Settings.CloseToTray) {
            E.Cancel = true;
            MinimiseToTray();
        }

        // Cleanup and dispose of the named pipe server
        if (_PipeServer is { } PS) {
            PS.Cancellation.Cancel();
            if (_PipeServerTask is { } Task) {
                Task.Wait();
            }
            _PipeServerTask = null;
            PS.Stream.Dispose();
        }
        _PipeServer = null;

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

    void MinimiseToTray() {
        Hide();
        TrayIcon.Visibility = Visibility.Visible;
    }

    #endregion

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
                Content               = $"A new version of {nameof(PlayRandom)} is available ({Release.Value.Version}).\nDo you want to download it?",
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
                Content               = "You are running the latest version of " + nameof(PlayRandom) + ".",
                Title                 = "No Update Available",
            };
            MBox.ButtonLeftClick += ( _, _ ) => MBox.Close();
            MBox.ButtonRightClick += ( _, _ ) => MBox.Close();
            MBox.ShowDialog();
        }
        Application.Current.Dispatcher.Invoke(Release is not null ? Callback_Available : Callback_NotAvailable);
    }
}
