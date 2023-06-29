namespace PlayRandom;

/// <summary> Common messages for piped communication. </summary>
public static class Messages {

    #region Identification

    /// <summary> The application name. </summary>
    public const string AppName = "PlayRandom";
    /// <summary> The COM application identifier. </summary>
    public const string ComAppIdentifier = "com." + AppIdentifier;
    /// <summary> The application identifier. </summary>
    public const string AppIdentifier = "github.neonalig.playrandom";
    /// <summary> The application description. </summary>
    public const string AppDescription = "Play playlists and folders both quickly, and randomly.";

    /// <summary> The prefix for global pipes. </summary>
    public const string GlobalPrefix = "Global\\";
    /// <summary> The prefix for local pipes. </summary>
    public const string LocalPrefix = "Local\\";

    /// <summary> The global mutex name. </summary>
    public const string GlobalPipe = GlobalPrefix + AppName;
    /// <summary> The local mutex name. </summary>
    public const string LocalPipe = LocalPrefix + AppName;

    /// <summary> Gets the global mutex name for the specified identifier. </summary>
    /// <param name="Identifier"> The identifier. </param>
    /// <returns> The global mutex name. </returns>
    [Pure, MustUseReturnValue] public static string GetGlobalMutexName( string Identifier ) => GlobalPrefix + Identifier;
    /// <summary> Gets the local mutex name for the specified identifier. </summary>
    /// <param name="Identifier"> The identifier. </param>
    /// <returns> The local mutex name. </returns>
    [Pure, MustUseReturnValue] public static string GetLocalMutexName( string Identifier ) => LocalPrefix + Identifier;

    #endregion

    #region Configuration

    /// <summary> The default timeout for connecting to a named pipe. </summary>
    public const int Timeout = 500;

    #endregion

    /// <summary> The message to bring the application to the foreground. </summary>
    public const string BringToFront = "BringToFront";
}
