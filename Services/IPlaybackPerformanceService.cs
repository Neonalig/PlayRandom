namespace PlayRandom.Services;

public interface IPlaybackPerformanceService {

    /// <summary> Opens the given <see cref="PlaybackOption"/>. </summary>
    /// <param name="Option"> The <see cref="PlaybackOption"/> to open. </param>
    /// <returns> <see cref="Success"/>, or an <see cref="Exception"/> if the performance failed. </returns>
    OneOf<Success, Exception> Open( PlaybackOption Option );

}
