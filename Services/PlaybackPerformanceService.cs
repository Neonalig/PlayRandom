namespace PlayRandom.Services;

public sealed class PlaybackPerformanceService : IPlaybackPerformanceService {

    #region Implementation of IPlaybackPerformanceService

    /// <inheritdoc />
    public OneOf<Success, Exception> Open( PlaybackOption Option ) {
        OneOf<Success, Exception> Result = Option.TryPickT0(out PlaylistFile File, out MusicFolder Folder)
            ? Settings.Executable.Launch(File)
            : Settings.Executable.Launch(Folder);
        if (Result.IsT0) {
            async Task DelayedExit() {
                await Task.Delay(100);
                // Application.Current.MainWindow.Close();
                Application.Current.Shutdown();
            }

            DelayedExit().Forget();
        }
        return Result;
    }

    #endregion

}
