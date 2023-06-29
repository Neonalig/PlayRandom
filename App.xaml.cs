namespace PlayRandom;

/// <summary> Interaction logic for App.xaml </summary>
public partial class App {

    /// <summary> Whether this is the first instance of the application. </summary>
    public static bool IsNewInstance { get; private set; } = true;

    /// <inheritdoc />
    protected override void OnStartup( StartupEventArgs E ) {
        if (Args.TryGetValue("set-startup", out string Value)) {
            Value = Value.ToLowerInvariant();
            Debug.WriteLine($"Setting startup to {Value}");
            switch (Value) {
                case "true" or "1":
                    PlayRandom.Startup.Set(true);
                    break;
                case "false" or "0":
                    PlayRandom.Startup.Set(false);
                    break;
                default:
                    Debug.WriteLine($"Invalid value '{Value}'");
                    break;
            }
        }

        using Mutex Mutex = new(true, Messages.GlobalPipe, out bool IsNew);
        IsNewInstance = IsNew;
        if (!IsNewInstance) {
            // Another instance is already running
            // Send a message to the running instance to bring it to the foreground
            bool Success;
            using (NamedPipeClientStream Client = new(".", Messages.LocalPipe, PipeDirection.Out)) {
                try {
                    Client.Connect(Messages.Timeout); // Timeout for connecting to the named pipe
                    StreamWriter Writer = new(Client);
                    Writer.WriteLine(Messages.BringToFront);
                    Writer.Flush();
                    Success = true;
                } catch {
                    // Failed to connect to the running instance
                    // Handle the error gracefully
                    WindowsMessageBox.Show(
                        "Another instance of PlayRandom is already running, but it could not be brought to the foreground.",
                        "PlayRandom",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    Success = false;
                }
            }

            if (Success) {
                // Terminate this instance since another one is already running
                Current.Shutdown();
                return;
            }
        }

        base.OnStartup(E);
    }

}
