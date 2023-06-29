using System.Collections.Specialized;
using PlayRandom.Services;
using PlayRandom.Views.Windows;

namespace PlayRandom.ViewModels;

/// <summary> View model for the <see cref="PlayRandom.Views.Pages.Dashboard"/> class. </summary>
public sealed class DashboardViewModel : ViewModel {
    /// <summary> The playback option service. </summary>
    readonly IPlaybackOptionService _PlaybackOptionService;

    /// <summary> The playback performance service. </summary>
    readonly IPlaybackPerformanceService _PlaybackPerformanceService = new PlaybackPerformanceService();

    /// <summary> The collection of playback options. </summary>
    public ObservableCollection<PlaybackOption> PlaybackOptions { get; } = new();

    /// <summary> The folder to search for the playback options. </summary>
    public string SearchPath { get; set; }

    /// <summary> The playback command. </summary>
    public ICommand PlaybackCommand { get; }

    /// <summary> The shuffle command. </summary>
    public ICommand ShuffleCommand { get; }

    /// <summary> The play first command. </summary>
    public ICommand PlayFirstCommand { get; }

    /// <summary> Initialises a new instance of the <see cref="DashboardViewModel"/> class. </summary>
    public DashboardViewModel() {
        _PlaybackOptionService = new PlaybackOptionService();

        PlaybackCommand = new RelayCommand<int>(Playback, CanPlayback);
        void Playback( int Index ) {
            PlaybackOption            PlaybackOption = PlaybackOptions[Index];
            OneOf<Success, Exception> Result         = _PlaybackPerformanceService.Open(PlaybackOption);
            if (Result.TryPickT1(out Exception Exception, out _)) {
                MainWindow.HandleException(Exception);
            }
        }
        bool CanPlayback( int Index ) => PlaybackOptions.Count > Index;
        PlayFirstCommand = new RelayCommand(PlayFirst);
        void PlayFirst() {
            if (PlaybackCommand.CanExecute(0)) {
                PlaybackCommand.Execute(0);
            }
        }

        ShuffleCommand = new RelayCommand(Shuffle, CanShuffle);
        void Shuffle() {
            PlaybackOptions.Shuffle();
        }
        bool CanShuffle() => PlaybackOptions.Count > 1;

        SearchPath = Settings.LastSearchPath;
        LoadPlaybackOptionsAsync().Forget();

        PropertyChanged += PropertyChangedCallback;
        static void PropertyChangedCallback( object? Sender, PropertyChangedEventArgs PropertyChangedEventArgs ) {
            if (!Settings.OfferToRememberSearchPath) { return; }
            if (Sender is not DashboardViewModel VM) { Debug.Fail("Sender is not " + nameof(DashboardViewModel)); return; }
            switch (PropertyChangedEventArgs.PropertyName) {
                case nameof(SearchPath): {
                    MessageBox MBox = new() {
                        ButtonLeftName        = "Yes",
                        ButtonLeftAppearance  = ControlAppearance.Primary,
                        ButtonRightName       = "No",
                        ButtonRightAppearance = ControlAppearance.Secondary,
                        Content               = "Remember this search path?",
                        Title                 = "Remember search path"
                    };
                    MBox.ButtonLeftClick += ( _, _ ) => {
                        Settings.LastSearchPath = VM.SearchPath;
                        MBox.Close();
                    };
                    MBox.ButtonRightClick += ( _, _ ) => {
                        MBox.Close();
                    };
                    MBox.ShowDialog();
                    VM.LoadPlaybackOptionsAsync().Forget();
                    break;
                }
            }
        }

        void OnPlaybackOptionsOnCollectionChanged( object? Sender, NotifyCollectionChangedEventArgs Args ) {
            FoundFiles = PlaybackOptions.Count;
            OnPropertyChanged(nameof(PlaybackOptions));
            ((RelayCommand)ShuffleCommand).NotifyCanExecuteChanged();
            ((RelayCommand)PlayFirstCommand).NotifyCanExecuteChanged();
        }
        PlaybackOptions.CollectionChanged += OnPlaybackOptionsOnCollectionChanged;
    }

    /// <summary> Current number found files. </summary>
    public int FoundFiles { get; private set; }

    /// <summary> Whether the view model is currently loading playback options. </summary>
    public bool IsLoading { get; private set; }

    /// <summary> Loads the playback options asynchronously. </summary>
    /// <param name="ClearExisting"> Whether to clear the existing playback options. </param>
    /// <param name="Token"> The cancellation token. </param>
    /// <returns> The task. </returns>
    public async Task LoadPlaybackOptionsAsync( bool ClearExisting = true, CancellationToken Token = default ) {
        if (!SearchPath.TryGetDirectoryInfo(out DirectoryInfo? Folder)) {
            Debug.WriteLine($"Invalid search path: {SearchPath}");
            return;
        }

        FoundFiles = 0;
        IsLoading  = true;
        if (ClearExisting) {
            PlaybackOptions.Clear();
        }
        async Task BackgroundTask() {
            await foreach (PlaybackOption PlaybackOption in _PlaybackOptionService.GetPlaybackOptionsAsync(Folder, Token)) {
                Application.Current.Dispatcher.Invoke(() => {
                    PlaybackOptions.Add(PlaybackOption);
                    FoundFiles++;
                });
            }

            Application.Current.Dispatcher.Invoke(() => {
                IsLoading = false;
            });
        }
        await Task.Run(BackgroundTask, Token);
    }
}
