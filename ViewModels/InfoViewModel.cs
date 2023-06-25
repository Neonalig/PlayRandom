using System.Reflection;
using PlayRandom.Models;
using PlayRandom.Views.Windows;

namespace PlayRandom.ViewModels;

public sealed class InfoViewModel : ViewModel {

    /// <summary> The version. </summary>
    public Version Version { get; private set; }

    /// <summary> The url to the currently installed GitHub release. </summary>
    public Uri VersionUri { get; private set; } = EmptyUri;

    /// <summary> The branch of the currently installed GitHub release. </summary>
    public string Branch { get; private set; } = string.Empty;

    /// <summary> The url to the currently installed GitHub release's branch. </summary>
    public Uri BranchUri { get; private set; } = EmptyUri;

    /// <summary> The 'More by Neonalig' command. </summary>
    public ICommand MoreCommand { get; }

    /// <summary> The 'Open GitHub' command. </summary>
    public ICommand OpenGitHubCommand { get; }

    /// <summary> The 'Check for updates' command. </summary>
    public ICommand CheckForUpdatesCommand { get; }

    /// <summary> The 'Buy me a coffee' command. </summary>
    public ICommand BuyMeACoffeeCommand { get; }

    /// <summary> The url to the GitHub repository. </summary>
    public Uri GitHubUri { get; private set; } = new("https://github.com/" + _GitHubOwner + "/" + _GitHubRepo);

    /// <summary> The url to the 'More by Neonalig' page. </summary>
    public Uri MoreUri { get; private set; } = new("https://linktr.ee/Neonalig");

    /// <summary> The url to the 'Buy me a coffee' page. </summary>
    public Uri BuyMeACoffeeUri { get; private set; } = new("https://bmc.link/neonalig");

    /// <summary> Whether an update is currently being checked for. </summary>
    public bool IsCheckingForUpdates { get; private set; }

    static RelayCommand CreateCommand( Uri Url ) {
        void Execute() => Url.OpenUrl();
        return new(Execute);
    }

    readonly GitHubClient _Client;

    const string
        _GitHubOwner = "Neonalig",
        _GitHubRepo  = nameof(PlayRandom);

    async Task CheckForUpdates() {
        IsCheckingForUpdates = true;
        try {
            IReadOnlyList<Release>        Releases = await _Client.Repository.Release.GetAll(_GitHubOwner, _GitHubRepo);
            IReadOnlyList<ScannedRelease> Scanned  = ScannedRelease.Scan(Releases);

            // Find current version
            if (Scanned.TryFind(Version, out ScannedRelease Current)) {
                VersionUri = Current.Uri;
                Branch     = Current.Branch;
                BranchUri  = Current.BranchUri;
            } else {
                VersionUri = EmptyUri;
                Branch     = string.Empty;
                BranchUri  = EmptyUri;
            }

            // Find latest version (scanned is from ordered by version)
            ScannedRelease Latest;
            if (Scanned.Count > 0 && (Latest = Scanned[^1]).Version != Version) {
                MainWindow.ShowUpdateAvailable(Latest);
            } else {
                MainWindow.ShowUpdateAvailable(null);
            }
        } catch (Exception Ex) {
            MainWindow.HandleException(Ex);
        } finally {
            IsCheckingForUpdates = false;
        }
    }

    bool CanCheckForUpdates() => !IsCheckingForUpdates;

    static Uri EmptyUri { get; } = new(string.Empty, UriKind.RelativeOrAbsolute);

    public InfoViewModel() {
        if (Assembly.GetEntryAssembly()?.GetName().Version is { } Version) {
            _Client      = new(new ProductHeaderValue(nameof(PlayRandom), Version.ToString()));
            this.Version = Version;
        } else {
            _Client      = new(new ProductHeaderValue(nameof(PlayRandom)));
            this.Version = new(0, 0);
        }

        MoreCommand            = CreateCommand(MoreUri);
        OpenGitHubCommand      = CreateCommand(GitHubUri);
        CheckForUpdatesCommand = new AsyncRelayCommand(CheckForUpdates, CanCheckForUpdates);
        BuyMeACoffeeCommand    = CreateCommand(BuyMeACoffeeUri);

        PropertyChanged += PropertyChangedCallback;
        static void PropertyChangedCallback( object? Sender, PropertyChangedEventArgs E ) {
            if (Sender is not InfoViewModel VM) { Debug.Fail("Sender is not " + nameof(InfoViewModel)); return; }
            switch (E.PropertyName) {
                case nameof(IsCheckingForUpdates):
                    ((AsyncRelayCommand)VM.CheckForUpdatesCommand).NotifyCanExecuteChanged();
                    break;
            }
        }
    }
}
