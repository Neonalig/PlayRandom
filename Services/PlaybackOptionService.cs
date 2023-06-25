namespace PlayRandom.Services;

/// <summary> Retrieves both <see cref="PlaylistFile"/>(s) and <see cref="MusicFolder"/>(s) from the current working directory. </summary>
public sealed class PlaybackOptionService : IPlaybackOptionService {
    /// <inheritdoc />
    public async IAsyncEnumerable<PlaybackOption> GetPlaybackOptionsAsync( DirectoryInfo Directory, [EnumeratorCancellation] CancellationToken Token = default ) {
        bool Any = false;
        await foreach (FileInfo File in Directory.EnumerateFilesAsync("*.*", SearchOption.AllDirectories, Token)) {
            foreach (PlaylistFormat Format in PlaylistFormat.List) {
                if (File.Extension.ToLower() == Format.Extension) {
                    Any = true;
                    yield return File;
                }
            }
        }

        if (!Any) {
            Debug.WriteLine($"\t{Directory.FullName} does not contain any playlists. Returning self.");
            yield return Directory;
        }
    }
}
