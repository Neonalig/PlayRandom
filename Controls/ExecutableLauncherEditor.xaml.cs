using PlayRandom.Models;

namespace PlayRandom.Controls;

public partial class ExecutableLauncherEditor : INotifyPropertyChanged {
    public ExecutableLauncherEditor() {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary> The executable launcher. </summary>
    public static readonly DependencyProperty ExecutableLauncherProperty = DependencyProperty.Register(nameof(ExecutableLauncher), typeof(ExecutableLauncher), typeof(ExecutableLauncherEditor), new(UpdateProperties));

    static void UpdateProperties( DependencyObject Sender, DependencyPropertyChangedEventArgs E ) {
        ExecutableLauncherEditor Self = (ExecutableLauncherEditor)Sender;
        Self.OnPropertyChanged(nameof(FilePath));
        Self.OnPropertyChanged(nameof(Arguments));
    }

    /// <summary> Gets or sets the executable launcher. </summary>
    public ExecutableLauncher? ExecutableLauncher {
        get => (ExecutableLauncher?)GetValue(ExecutableLauncherProperty);
        set => SetValue(ExecutableLauncherProperty, value);
    }

    /// <summary> The label width. </summary>
    public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(nameof(LabelWidth), typeof(double), typeof(ExecutableLauncherEditor), new(100d));

    /// <summary> Gets or sets the label width. </summary>
    public double LabelWidth {
        get => (double)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    /// <summary> Gets or sets the file path for the executable. </summary>
    public string FilePath {
        get => ExecutableLauncher is { } Launcher ? Launcher.Executable : string.Empty;
        set {
            if (ExecutableLauncher is { } Launcher) {
                ExecutableLauncher = new(value, Launcher.Arguments);
            } else {
                ExecutableLauncher = new(value);
            }
        }
    }

    /// <summary> Gets or sets the arguments for the executable. </summary>
    public string Arguments {
        get => ExecutableLauncher is { } Launcher ? Launcher.Arguments : string.Empty;
        set {
            if (ExecutableLauncher is { } Launcher) {
                ExecutableLauncher = new(Launcher.Executable, value);
            } else {
                ExecutableLauncher = new(GetFallbackExecutable(), value);
            }
        }
    }

    static string GetFallbackExecutable() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return Path.Combine(Environment.SystemDirectory, "cmd.exe");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            return "/bin/sh";
        }

        throw new PlatformNotSupportedException();
    }

    #region INotifyPropertyChanged Implementation

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary> Raises the <see cref="PropertyChanged"/> event. </summary>
    /// <param name="PropertyName"> The name of the property that changed. </param>
    void OnPropertyChanged( [CallerMemberName] string? PropertyName = null ) => PropertyChanged?.Invoke(this, new(PropertyName));

    #endregion

}
