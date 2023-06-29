namespace PlayRandom;

/// <summary> Represents a folder containing music. </summary>
/// <param name="Folder"> The folder containing the music. </param>
[DebuggerDisplay("{Name} ({Folder.FullName})")]
public readonly record struct MusicFolder( DirectoryInfo Folder ) {

    /// <inheritdoc cref="FileSystemInfo.Name"/>
    public string Name => Folder.Name;

    /// <inheritdoc cref="FileSystemInfo.FullName"/>
    public string FullName => Folder.FullName;

    /// <summary> Gets the parent folder of the file or folder. </summary>
    public DirectoryInfo Parent => Folder.Parent!;

    public static implicit operator DirectoryInfo( MusicFolder Folder ) => Folder.Folder;
    public static implicit operator MusicFolder( DirectoryInfo Folder ) => new( Folder );

    #region Overrides of ValueType

    /// <inheritdoc />
    public override string ToString() => Folder.ToString();

    #endregion

}
