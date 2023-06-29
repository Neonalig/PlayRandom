namespace PlayRandom;

public static class FileSystemInfoExtensions {

    /// <summary> Enumerates the files in a directory asynchronously. </summary>
    /// <param name="Directory"> The directory to enumerate. </param>
    /// <param name="CancellationToken"> The cancellation token to cancel the operation. </param>
    /// <returns> An asynchronous enumerable of files in the directory. </returns>
    public static async IAsyncEnumerable<FileInfo> EnumerateFilesAsync( this DirectoryInfo Directory, [EnumeratorCancellation] CancellationToken CancellationToken = default ) {
        foreach (FileInfo File in Directory.EnumerateFiles()) {
            await Task.Yield();
            yield return File;
            CancellationToken.ThrowIfCancellationRequested();
        }
    }

    /// <summary> Enumerates the files in a directory asynchronously. </summary>
    /// <param name="Directory"> The directory to enumerate. </param>
    /// <param name="SearchPattern"> The search string to match against the names of files in the directory. </param>
    /// <param name="EnumerationOptions"> The options to use for the enumeration. </param>
    /// <param name="CancellationToken"> The cancellation token to cancel the operation. </param>
    /// <returns> An asynchronous enumerable of files in the directory. </returns>
    public static async IAsyncEnumerable<FileInfo> EnumerateFilesAsync( this DirectoryInfo Directory, string SearchPattern, EnumerationOptions EnumerationOptions, [EnumeratorCancellation] CancellationToken CancellationToken = default ) {
        foreach (FileInfo File in Directory.EnumerateFiles(SearchPattern, EnumerationOptions)) {
            await Task.Yield();
            yield return File;
            CancellationToken.ThrowIfCancellationRequested();
        }
    }

    /// <summary> Enumerates the files in a directory asynchronously. </summary>
    /// <param name="Directory"> The directory to enumerate. </param>
    /// <param name="SearchPattern"> The search string to match against the names of files in the directory. </param>
    /// <param name="SearchOption"> One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories. </param>
    /// <param name="CancellationToken"> The cancellation token to cancel the operation. </param>
    /// <returns> An asynchronous enumerable of files in the directory. </returns>
    public static async IAsyncEnumerable<FileInfo> EnumerateFilesAsync( this DirectoryInfo Directory, string SearchPattern, SearchOption SearchOption, [EnumeratorCancellation] CancellationToken CancellationToken = default ) {
        foreach (FileInfo File in Directory.EnumerateFiles(SearchPattern, SearchOption)) {
            await Task.Yield();
            yield return File;
            CancellationToken.ThrowIfCancellationRequested();
        }
    }

    /// <summary> Enumerates the files in a directory asynchronously. </summary>
    /// <param name="Directory"> The directory to enumerate. </param>
    /// <param name="SearchPattern"> The search string to match against the names of files in the directory. </param>
    /// <param name="CancellationToken"> The cancellation token to cancel the operation. </param>
    /// <returns> An asynchronous enumerable of files in the directory. </returns>
    public static async IAsyncEnumerable<FileInfo> EnumerateFilesAsync( this DirectoryInfo Directory, string SearchPattern, [EnumeratorCancellation] CancellationToken CancellationToken = default ) {
        foreach (FileInfo File in Directory.EnumerateFiles(SearchPattern)) {
            await Task.Yield();
            yield return File;
            CancellationToken.ThrowIfCancellationRequested();
        }
    }

    /// <summary> Attempts to get the file information for a given file path. </summary>
    /// <param name="Path"> The path to the file. </param>
    /// <param name="File"> [out] The file information for the given path. </param>
    /// <returns> <see langword="true"/> if the file information was successfully retrieved; otherwise, <see langword="false"/>. </returns>
    public static bool TryGetFileInfo( this string? Path, [NotNullWhen(true)] out FileInfo? File ) {
        if (string.IsNullOrWhiteSpace(Path)) {
            File = null;
            return false;
        }
        try {
            File = new(Path);
            return true;
        } catch {
            File = null;
            return false;
        }
    }

    /// <summary> Attempts to get the directory information for a given directory path. </summary>
    /// <param name="Path"> The path to the directory. </param>
    /// <param name="Directory"> [out] The directory information for the given path. </param>
    /// <returns> <see langword="true"/> if the directory information was successfully retrieved; otherwise, <see langword="false"/>. </returns>
    public static bool TryGetDirectoryInfo( this string? Path, [NotNullWhen(true)] out DirectoryInfo? Directory ) {
        if (string.IsNullOrWhiteSpace(Path)) {
            Directory = null;
            return false;
        }
        try {
            Directory = new(Path);
            return true;
        } catch {
            Directory = null;
            return false;
        }
    }

    /// <summary> Attempts to get the drive information for a given file/folder. </summary>
    /// <param name="Path"> The path to get the drive information for. </param>
    /// <param name="Drive"> [out] The drive information for the given path. </param>
    /// <returns> <see langword="true"/> if the drive information was successfully retrieved; otherwise, <see langword="false"/>. </returns>
    public static bool TryGetDriveInfo( this FileSystemInfo Path, [NotNullWhen(true)] out DriveInfo? Drive ) {
        if (Path is not FileInfo and not DirectoryInfo) {
            Drive = null;
            return false;
        }

        string? PathRoot = System.IO.Path.GetPathRoot(Path.FullName);
        if (PathRoot is null) {
            Drive = null;
            return false;
        }

        try {
            Drive = new(PathRoot);
            return true;
        } catch {
            Drive = null;
            return false;
        }
    }

    /// <summary> Gets the drive information for a given file/folder. </summary>
    /// <param name="Path"> The path to get the drive information for. </param>
    /// <returns> The drive information for the given path. </returns>
    public static DriveInfo GetDriveInfo( this FileSystemInfo Path ) {
        Debug.Assert(Path is FileInfo or DirectoryInfo, "Path is not a file or directory.");
        string? PathRoot = System.IO.Path.GetPathRoot(Path.FullName);
        if (PathRoot is null) {
            throw new ArgumentException("Path is not a rooted path.", nameof(Path));
        }

        return new(PathRoot);
    }

    /// <summary> Adds quotes around the path if required. </summary>
    /// <param name="Path"> The path to quote. </param>
    /// <returns> The quoted path. </returns>
    [Pure]
    public static string Quote( this string Path ) {
        if (string.IsNullOrWhiteSpace(Path)) {
            return "\"\"";
        }

        if (Path.StartsWith('\"') && Path.EndsWith('\"')) {
            return Path;
        }

        if (Path.Contains(' ')) {
            return $"\"{Path}\"";
        }

        return Path;
    }
}
