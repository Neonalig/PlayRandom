using PlayRandom.Converters;
using PlayRandom.Models;
using PlayRandom.Views.Windows;

namespace PlayRandom.ViewModels;

/// <summary> View model for the <see cref="PlayRandom.Views.Pages.Settings"/> class. </summary>
public sealed class SettingsViewModel : ViewModel {

    /// <summary> Initialises a new instance of the <see cref="SettingsViewModel"/> class. </summary>
    public SettingsViewModel() {
        LoadFromSettings();

        PropertyChanged += PropertyChangedCallback;
        static void PropertyChangedCallback( object? Sender, PropertyChangedEventArgs E ) {
            if (Sender is not SettingsViewModel VM) { Debug.Fail("Sender is not " + nameof(SettingsViewModel)); return; }
            switch (E.PropertyName) {
                case nameof(Executable):
                    Settings.Executable = VM.Executable;
                    break;
                case nameof(OfferToRememberSearchPath):
                    Settings.OfferToRememberSearchPath = VM.OfferToRememberSearchPath;
                    break;
                case nameof(LastSearchPath):
                    Settings.LastSearchPath = VM.LastSearchPath;
                    break;
                case nameof(StartWithOS):
                    Settings.StartWithOS = VM.StartWithOS;
                    break;
                case nameof(MinimiseToTray):
                    Settings.MinimiseToTray = VM.MinimiseToTray;
                    break;
                case nameof(CloseToTray):
                    Settings.CloseToTray = VM.CloseToTray;
                    break;
                case nameof(AlwaysOnTop):
                    Settings.AlwaysOnTop = VM.AlwaysOnTop;
                    break;
                case nameof(IsModified):
                    ((AsyncRelayCommand)VM.SaveCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)VM.RevertCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)VM.ResetCommand).NotifyCanExecuteChanged();
                    break;
                default:
                    Debug.Fail($"Unexpected property name: {E.PropertyName}");
                    break;
            }
        }

        Settings.Changed += SettingsOnChanged;
        void SettingsOnChanged( string Name, object Value ) => LoadFromSettings();

        SaveCommand = new AsyncRelayCommand(Save, CanSave);
        async Task Save( CancellationToken Token ) {
            try {
                await Settings.SaveAsync(Token);
                LoadFromSettings();
            } catch (Exception Ex) {
                MainWindow.HandleException(Ex);
            }
        }
        bool CanSave() => Settings.HasUnsavedChanges;

        RevertCommand = new AsyncRelayCommand(Revert, CanRevert);
        async Task Revert( CancellationToken Token ) {
            try {
                await Settings.RevertAsync(Token);
                LoadFromSettings();
            } catch (Exception Ex) {
                MainWindow.HandleException(Ex);
            }
        }
        bool CanRevert() => Settings.HasUnsavedChanges;

        ResetCommand = new AsyncRelayCommand(Reset, CanReset);
        async Task Reset( CancellationToken Token ) {
            try {
                await Settings.ClearAsync(Token);
                LoadFromSettings();
            } catch (Exception Ex) {
                MainWindow.HandleException(Ex);
            }
        }
        bool CanReset() => Settings.Any;
    }

    [MemberNotNull(nameof(Executable), nameof(OfferToRememberSearchPath), nameof(LastSearchPath))]
    void LoadFromSettings() {
        Executable                = Settings.Executable;
        OfferToRememberSearchPath = Settings.OfferToRememberSearchPath;
        LastSearchPath            = Settings.LastSearchPath;
        StartWithOS               = Settings.StartWithOS;
        MinimiseToTray            = Settings.MinimiseToTray;
        CloseToTray               = Settings.CloseToTray;
        AlwaysOnTop               = Settings.AlwaysOnTop;
        OnPropertyChanged(nameof(IsModified));
    }

    /// <summary> Gets or sets the executable to launch when performing a playback. </summary>
    public ExecutableLauncher Executable { get; set; }

    /// <summary> Gets or sets a value indicating whether to remember the search paths. </summary>
    public bool OfferToRememberSearchPath { get; set; }

    /// <summary> Gets or sets the default search path. </summary>
    public string LastSearchPath { get; set; }

    /// <summary> Gets or sets whether to start with the operating system. </summary>
    public bool StartWithOS { get; set; }

    /// <summary> Gets or sets whether to minimise to the system tray. </summary>
    public bool MinimiseToTray { get; set; }

    /// <summary> Gets or sets whether to minimise to the system tray when the window is closed. </summary>
    public bool CloseToTray { get; set; }

    /// <summary> Gets or sets whether to stay topmost. </summary>
    public bool AlwaysOnTop { get; set; }

    /// <summary> Gets the platform identifier. </summary>
    #pragma warning disable CA1822
    public PlatformID Platform => Environment.OSVersion.Platform;
    #pragma warning restore CA1822

    /// <summary> Gets the command to save the settings. </summary>
    public ICommand SaveCommand { get; }

    /// <summary> Gets the command to revert the settings. </summary>
    public ICommand RevertCommand { get; }

    /// <summary> Gets the command to reset the settings. </summary>
    public ICommand ResetCommand { get; }

    /// <summary> Gets whether the settings have been modified. </summary>
    #pragma warning disable CA1822
    public bool IsModified => Settings.HasUnsavedChanges;
    #pragma warning restore CA1822

    /// <summary> The tooltip for the <see cref="StartWithOS"/> property. </summary>
    public static string StartWithOSTooltip { get; } = $"Whether to start with {PlatformToStringConverter.String}";
}
