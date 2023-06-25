namespace PlayRandom;

/// <summary> Represents a file containing a playlist. </summary>
/// <param name="File"> The file containing the playlist. </param>
[DebuggerDisplay("{Name} ({FullName})")]
public readonly record struct PlaylistFile( FileInfo File ) {

    /// <inheritdoc cref="FileSystemInfo.Name"/>
    public string Name => File.Name;

    /// <inheritdoc cref="FileSystemInfo.FullName"/>
    public string FullName => File.FullName;

    /// <inheritdoc cref="FileInfo.Directory"/>
    public DirectoryInfo Directory => File.Directory!;

    public static implicit operator FileInfo( PlaylistFile File ) => File.File;
    public static implicit operator PlaylistFile( FileInfo File ) => new( File );

}
