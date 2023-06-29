namespace PlayRandom;

[DebuggerDisplay("{FileName} ({FullName})")]
public readonly struct PlaybackOption : IEquatable<PlaybackOption>, IComparable<PlaybackOption>, IComparable {

    readonly PlaylistFile? _File;
    readonly MusicFolder?  _Folder;
    readonly bool          _IsFile;

    public PlaybackOption( PlaylistFile File ) {
        _File   = File;
        _Folder = null;
        _IsFile = true;
    }

    public PlaybackOption( MusicFolder Folder ) {
        _File   = null;
        _Folder = Folder;
        _IsFile = false;
    }

    /// <summary> Attempts to get the playlist file. </summary>
    /// <param name="File"> The file, or <see langword="null"/> if the option is a folder. </param>
    /// <returns> <see langword="true"/> if the option is a file; otherwise, <see langword="false"/>. </returns>
    public bool TryGetFile( [NotNullWhen(true)] out PlaylistFile? File ) {
        File = _File;
        return _IsFile;
    }

    /// <inheritdoc cref="TryGetFile(out PlaylistFile?)"/>
    /// <param name="File"> The file, or <see langword="null"/> if the option is a folder. </param>
    /// <param name="Folder"> The folder, or <see langword="null"/> if the option is a file. </param>
    public bool TryGetFile( [NotNullWhen(true)] out PlaylistFile? File, [NotNullWhen(false)] out MusicFolder? Folder ) {
        File   = _File;
        Folder = _Folder;
        return _IsFile;
    }

    /// <summary> Attempts to get the music folder. </summary>
    /// <param name="Folder"> The folder, or <see langword="null"/> if the option is a file. </param>
    /// <returns> <see langword="true"/> if the option is a folder; otherwise, <see langword="false"/>. </returns>
    public bool TryGetFolder( [NotNullWhen(true)] out MusicFolder? Folder ) {
        Folder = _Folder;
        return !_IsFile;
    }

    /// <inheritdoc cref="TryGetFolder(out MusicFolder?)"/>
    /// <param name="File"> The file, or <see langword="null"/> if the option is a folder. </param>
    /// <param name="Folder"> The folder, or <see langword="null"/> if the option is a file. </param>
    public bool TryGetFolder( [NotNullWhen(true)] out PlaylistFile? File, [NotNullWhen(false)] out MusicFolder? Folder ) {
        File   = _File;
        Folder = _Folder;
        return !_IsFile;
    }

    /// <summary> Gets the file. </summary>
    /// <returns> The file. </returns>
    /// <exception cref="InvalidOperationException"> The option is a folder. </exception>
    public PlaylistFile GetFile() => _IsFile ? _File!.Value : throw new InvalidOperationException("The option is a folder.");

    /// <summary> Gets the folder. </summary>
    /// <returns> The folder. </returns>
    /// <exception cref="InvalidOperationException"> The option is a file. </exception>
    public MusicFolder GetFolder() => !_IsFile ? _Folder!.Value : throw new InvalidOperationException("The option is a file.");

    /// <summary> Gets the name of the file or folder. </summary>
    public string Name => TryGetFile(out PlaylistFile? File, out MusicFolder? Folder) ? Path.GetFileNameWithoutExtension(File.Value.Name) : Folder.Value.Name;

    /// <inheritdoc cref="FileSystemInfo.Name"/>
    public string FileName => TryGetFile(out PlaylistFile? File, out MusicFolder? Folder) ? File.Value.Name : Folder.Value.Name;

    /// <inheritdoc cref="FileSystemInfo.FullName"/>
    public string FullName => TryGetFile(out PlaylistFile? File, out MusicFolder? Folder) ? File.Value.FullName : Folder.Value.FullName;

    /// <summary> Gets the parent folder of the file or folder. </summary>
    public DirectoryInfo Parent => TryGetFile(out PlaylistFile? File, out MusicFolder? Folder) ? File.Value.Directory : Folder.Value.Parent;

    public static implicit operator FileSystemInfo( PlaybackOption Option ) => Option.TryGetFile(out PlaylistFile? File, out MusicFolder? Folder) ? File.Value : Folder.Value;

    public static implicit operator PlaybackOption( FileInfo      File )   => (PlaylistFile)File;
    public static implicit operator PlaybackOption( DirectoryInfo Folder ) => (MusicFolder)Folder;

    public static implicit operator PlaybackOption( PlaylistFile File )   => new(File);
    public static explicit operator PlaylistFile( PlaybackOption Option ) => Option.GetFile();

    public static implicit operator PlaybackOption( MusicFolder Folder ) => new(Folder);
    public static explicit operator MusicFolder( PlaybackOption Option ) => Option.GetFolder();

    #region Overrides of ValueType

    /// <inheritdoc />
    public override string ToString() => TryGetFile(out PlaylistFile? File, out MusicFolder? Folder) ? File.Value.ToString() : Folder.Value.ToString();

    #endregion

    #region Equality Members

    /// <inheritdoc />
    public bool Equals( PlaybackOption Other ) => _IsFile ? Other._IsFile && _File!.Equals(Other._File) : !Other._IsFile && _Folder!.Equals(Other._Folder);

    /// <inheritdoc />
    public override bool Equals( object? Obj ) => Obj is PlaybackOption Other && Equals(Other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_File, _Folder, _IsFile);

    public static bool operator ==( PlaybackOption Left, PlaybackOption Right ) => Left.Equals(Right);
    public static bool operator !=( PlaybackOption Left, PlaybackOption Right ) => !Left.Equals(Right);

    #endregion

    #region Relational Members

    /// <inheritdoc />
    public int CompareTo( PlaybackOption Other ) {
        // Folders come first (alphabetical), then files (alphabetical)
        if (_IsFile) {
            if (Other._IsFile) {
                // return _File!.Value.CompareTo(Other._File!.Value);
                return Nullable.Compare(_File, Other._File);
            }

            return -1;
        }

        if (Other._IsFile) {
            return 1;
        }

        return Nullable.Compare(_Folder, Other._Folder);
    }

    /// <inheritdoc />
    public int CompareTo( object? Obj ) {
        if (ReferenceEquals(null, Obj)) {
            return 1;
        }

        return Obj is PlaybackOption Other ? CompareTo(Other) : throw new ArgumentException($"Object must be of type {nameof(PlaybackOption)}");
    }

    public static bool operator <( PlaybackOption  Left, PlaybackOption Right ) => Left.CompareTo(Right) < 0;
    public static bool operator >( PlaybackOption  Left, PlaybackOption Right ) => Left.CompareTo(Right) > 0;
    public static bool operator <=( PlaybackOption Left, PlaybackOption Right ) => Left.CompareTo(Right) <= 0;
    public static bool operator >=( PlaybackOption Left, PlaybackOption Right ) => Left.CompareTo(Right) >= 0;

    #endregion

}
