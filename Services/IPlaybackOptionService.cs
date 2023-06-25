using System.Diagnostics;

namespace PlayRandom.Services;

/// <summary> Represents a playback option retriever. </summary>
public interface IPlaybackOptionService {
    /// <summary> Gets the playback options asynchronously. </summary>
    /// <param name="Directory"> The directory to search. </param>
    /// <param name="CancellationToken"> The cancellation token. </param>
    /// <returns> The playback options. </returns>
    IAsyncEnumerable<PlaybackOption> GetPlaybackOptionsAsync( DirectoryInfo Directory, CancellationToken CancellationToken = default );
}
