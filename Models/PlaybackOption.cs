namespace PlayRandom;

[GenerateOneOf,
 DebuggerDisplay("{FileName} ({FullName})")]
public partial class PlaybackOption : OneOfBase<PlaylistFile, MusicFolder> {

    /// <summary> Gets the name of the file or folder. </summary>
    public string Name => TryPickT0(out PlaylistFile File, out MusicFolder Folder) ? Path.GetFileNameWithoutExtension(File.Name) : Folder.Name;

    /// <inheritdoc cref="FileSystemInfo.Name"/>
    public string FileName => TryPickT0(out PlaylistFile File, out MusicFolder Folder) ? File.Name : Folder.Name;

    /// <inheritdoc cref="FileSystemInfo.FullName"/>
    public string FullName => TryPickT0(out PlaylistFile File, out MusicFolder Folder) ? File.FullName : Folder.FullName;

    /// <summary> Gets the parent folder of the file or folder. </summary>
    public DirectoryInfo Parent => TryPickT0(out PlaylistFile File, out MusicFolder Folder) ? File.Directory : Folder.Parent;

    public static implicit operator FileSystemInfo( PlaybackOption Option ) => Option.TryPickT0(out PlaylistFile File, out MusicFolder Folder) ? File : Folder;

    public static implicit operator PlaybackOption( FileInfo      File )   => (PlaylistFile)File;
    public static implicit operator PlaybackOption( DirectoryInfo Folder ) => (MusicFolder)Folder;

}
