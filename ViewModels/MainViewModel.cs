namespace PlayRandom.ViewModels;

/// <summary> View model for the <see cref="PlayRandom.Views.Windows.MainWindow"/> class. </summary>
public sealed class MainViewModel : ViewModel {

    public MainViewModel() {
        Settings.Changed += OnSettingsChanged;

        void OnSettingsChanged( string Name, object Value ) {
            if (Name == nameof(Settings.AlwaysOnTop)) {
                if (Value is bool AOT) {
                    AlwaysOnTop = AOT;
                } else {
                    Debug.Fail(nameof(Settings) + "." + nameof(Settings.AlwaysOnTop) + " is not a boolean.");
                }
            }
        }
    }

    /// <summary> Gets or sets whether the window is always on top. </summary>
    public bool AlwaysOnTop {
        get => Settings.AlwaysOnTop;
        set {
            if (Settings.AlwaysOnTop == value) { return; }

            Settings.AlwaysOnTop = value;
            OnPropertyChanged();
        }
    }

}
