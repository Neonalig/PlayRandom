namespace PlayRandom.Models;

public sealed class ExecutableLauncher {
    /// <summary> The executable to launch. </summary>
    [JsonPropertyName("path")]
    public readonly string Executable;

    /// <summary> The arguments to pass to the executable. </summary>
    /// <remarks> {0} = File/folder path, <br/>
    /// {1} = File/folder name, <br/>
    /// {2} = Parent directory </remarks>
    [JsonPropertyName("args")]
    public readonly string Arguments;

    /// <summary> Initialises a new instance of the <see cref="ExecutableLauncher"/> struct. </summary>
    /// <param name="Executable"> The executable to launch. </param>
    /// <param name="Arguments"> The arguments to pass to the executable. <br/><br/>
    /// {0} = File/folder path, <br/>
    /// {1} = File/folder name, <br/>
    /// {2} = Parent directory </param>
    [JsonConstructor]
    public ExecutableLauncher( string Executable, string Arguments = "{0}" ) {
        this.Executable = Executable;
        this.Arguments  = Arguments;
    }

    static string GetAsArgument( string? Text ) => string.IsNullOrEmpty(Text) ? string.Empty : Text.Contains(' ') ? $"\"{Text}\"" : Text;

    /// <summary> Launches the executable. </summary>
    /// <param name="File"> The file to pass to the executable. </param>
    /// <returns> <see cref="Success"/>, or an <see cref="Exception"/> if the executable failed to launch. </returns>
    public OneOf<Success, Exception> Launch( FileInfo File ) {
        try {
            string Arguments = this.Arguments
                .Replace("{0}", GetAsArgument(File.FullName))
                .Replace("{1}", GetAsArgument(File.Name))
                .Replace("{2}", GetAsArgument(File.DirectoryName));
            Debug.WriteLine($"Launching {GetAsArgument(Executable)} {Arguments}...");
            Process.Start(Executable, Arguments);
            return new Success();
        } catch (Exception Exception) {
            return Exception;
        }
    }

    /// <summary> Launches the executable. </summary>
    /// <param name="Folder"> The folder to pass to the executable. </param>
    /// <returns> <see cref="Success"/>, or an <see cref="Exception"/> if the executable failed to launch. </returns>
    public OneOf<Success, Exception> Launch( DirectoryInfo Folder ) {
        try {
            string Arguments = this.Arguments
                .Replace("{0}", GetAsArgument(Folder.FullName))
                .Replace("{1}", GetAsArgument(Folder.Name))
                .Replace("{2}", GetAsArgument(Folder.Parent is { } Parent ? Parent.FullName : Folder.Root.FullName));
            Debug.WriteLine($"Launching {GetAsArgument(Executable)} {Arguments}...");
            Process.Start(Executable, Arguments);
            return new Success();
        } catch (Exception Exception) {
            return Exception;
        }
    }

    /// <summary> Attempts to get the system default for the given format. </summary>
    /// <param name="Format"> The format to get the system default for. </param>
    /// <param name="SystemDefault"> [out] The system default for the given format. </param>
    /// <returns> <see langword="true"/> if the system default was found; otherwise, <see langword="false"/>. </returns>
    public static bool TryGetSystemDefault( string Format, [MaybeNullWhen(false)] out ExecutableLauncher SystemDefault ) {
        using RegistryKey? Key = Registry.ClassesRoot.OpenSubKey(Format + @"\shell\open\command");
        if (Key is null) {
            SystemDefault = null;
            return false;
        }

        if (Key.GetValue(null) is not string Command) {
            SystemDefault = null;
            return false;
        }

        int SpaceIndex = Command.IndexOf(' ');
        if (SpaceIndex == -1) {
            SystemDefault = null;
            return false;
        }

        string Executable = Command[..SpaceIndex];
        string Arguments  = Command[(SpaceIndex + 1)..];
        SystemDefault = new(new(Executable), string.IsNullOrEmpty(Arguments) ? "{0}" : Arguments.Replace("%1", "{0}"));
        return true;
    }

    /// <summary> Gets the Windows-specific automatic determination fallback. </summary>
    public static ExecutableLauncher WindowsAutoFallback => new("C:\\Windows\\System32\\cmd.exe", "/c start {0}");

    /// <summary> Gets the Linux-specific automatic determination fallback. </summary>
    public static ExecutableLauncher LinuxAutoFallback => new("/usr/bin/xdg-open", "{0}");

    /// <summary> Gets the Mac-specific automatic determination fallback. </summary>
    public static ExecutableLauncher MacAutoFallback => new("/usr/bin/open", "{0}");

    /// <summary> Attempts to get the system default for the given format, using one of the automatic determination fallbacks if it fails. </summary>
    /// <param name="Format"> The format to get the system default for. </param>
    /// <returns> The system default for the given format. </returns>
    /// <exception cref="PlatformNotSupportedException"> Thrown when the operating system is not supported. </exception>
    public static ExecutableLauncher GetSystemDefault( string Format ) {
        if (TryGetSystemDefault(Format, out ExecutableLauncher? SystemDefault)) {
            Debug.WriteLine($"System default for {Format} is {SystemDefault}.");
            return SystemDefault;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Debug.WriteLine($"System default for {Format} not found, using Windows fallback.");
            return WindowsAutoFallback;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            Debug.WriteLine($"System default for {Format} not found, using Mac fallback.");
            return MacAutoFallback;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            Debug.WriteLine($"System default for {Format} not found, using Linux fallback.");
            return LinuxAutoFallback;
        }

        throw new PlatformNotSupportedException();
    }

    #region Overrides of ValueType

    /// <inheritdoc />
    public override string ToString() => $"{Executable} {Arguments}";

    #endregion
}
