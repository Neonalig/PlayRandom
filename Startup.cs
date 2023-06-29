using System.Reflection;

namespace PlayRandom;

public static class Startup {
    /// <summary> Sets whether the application should start on system startup. </summary>
    /// <param name="Enable"> Whether to enable or disable startup. </param>
    /// <exception cref="NotSupportedException"> Thrown when the operating system is not supported. </exception>
    public static void Set( bool Enable ) {
        if (!IsRunAsAdmin()) {
            Restart(Elevated: true, "--set-startup", Enable.ToString());
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Set_Windows(Enable);
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            Set_Mac(Enable);
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            Set_Linux(Enable);
        } else {
            throw new NotSupportedException("Unsupported operating system.");
        }
    }

    const string _WindowsRunKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

    static void Set_Windows( bool Enable ) {
        using RegistryKey? Key = Registry.CurrentUser.OpenSubKey(_WindowsRunKey, true);
        if (Key is not null) {
            if (Enable) {
                Debug.WriteLine($"Setting {Key}:{Messages.AppName} to {PathToApp}");
                Key.SetValue(Messages.AppName, PathToApp);
            } else {
                Debug.WriteLine($"Deleting {Key}:{Messages.AppName}");
                Key.DeleteValue(Messages.AppName, false);
            }
        } else {
            throw new InvalidOperationException("Could not open the registry key.");
        }
    }

    // static string PathToApp => Assembly.GetEntryAssembly()?.Location ?? throw new InvalidOperationException("Could not get the path to the application.");
    static string PathToApp {
        get {
            string Path;
            if (Assembly.GetEntryAssembly() is { } Entry) {
                Path = Entry.Location;
            } else if (Assembly.GetExecutingAssembly() is { } Executing) {
                Path = Executing.Location;
            } else {
                throw new InvalidOperationException("Could not get the path to the application.");
            }
            if (Path.EndsWith(".dll")) {
                Path = Path[..^3] + "exe";
            }
            return Path;
        }
    }

    static void Set_Mac( bool Enable ) {
        const string PlistPath = "~/Library/LaunchAgents/" + Messages.ComAppIdentifier + ".plist";
        string       FullPath  = Environment.ExpandEnvironmentVariables(PlistPath);

        if (Enable) {
            string PlistContent = GetMacPlistContent(PathToApp);

            File.WriteAllText(FullPath, PlistContent);
        } else {
            File.Delete(FullPath);
        }
    }

    static void Set_Linux( bool Enable ) {
        const string AutostartDirPath = "~/.config/autostart";
        string       FullPath         = Environment.ExpandEnvironmentVariables(AutostartDirPath);
        string       DesktopFilePath  = Path.Combine(FullPath, Messages.AppIdentifier + ".desktop");

        if (Enable) {
            string DesktopFileContent = GetLinuxDesktopFileContent(PathToApp);

            Directory.CreateDirectory(FullPath);
            File.WriteAllText(DesktopFilePath, DesktopFileContent);
        } else {
            File.Delete(DesktopFilePath);
        }
    }

    static string GetMacPlistContent( string AppPath ) =>
        $@"<?xml version=""1.0"" encoding=""UTF-8""?>
            <plist version=""1.0"">
                <dict>
                    <key>Label</key>
                    <string>{Messages.ComAppIdentifier}</string>
                    <key>ProgramArguments</key>
                    <array>
                        <string>{AppPath}</string>
                    </array>
                    <key>RunAtLoad</key>
                    <true/>
                </dict>
            </plist>";

    static string GetLinuxDesktopFileContent( string AppPath ) =>
        $@"[Desktop Entry]
            Version=1.0
            Name={Messages.AppName}
            Comment={Messages.AppDescription}
            Exec={AppPath}
            Terminal=false
            Type=Application
            Categories=Utility;Application;";

    /// <summary> Gets whether the application is set to start on system startup. </summary>
    /// <returns> Whether the application is set to start on system startup. </returns>
    /// <exception cref="NotSupportedException"> Thrown when the operating system is not supported. </exception>
    public static bool Get() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return Get_Windows();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            return Get_Mac();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            return Get_Linux();
        }

        throw new NotSupportedException("Unsupported operating system.");
    }

    static bool Get_Windows() {
        using RegistryKey? Key = Registry.CurrentUser.OpenSubKey(_WindowsRunKey, true);
        return Key?.GetValue(Messages.AppName) != null;
    }

    static bool Get_Mac() {
        const string PlistPath = "~/Library/LaunchAgents/" + Messages.ComAppIdentifier + ".plist";
        string       FullPath  = Environment.ExpandEnvironmentVariables(PlistPath);

        return File.Exists(FullPath);
    }

    static bool Get_Linux() {
        const string AutostartDirPath = "~/.config/autostart";
        string       FullPath         = Environment.ExpandEnvironmentVariables(AutostartDirPath);
        string       DesktopFilePath  = Path.Combine(FullPath, Messages.AppIdentifier + ".desktop");

        return File.Exists(DesktopFilePath);
    }

    /// <summary> Gets the type of startup that the application was launched with. </summary>
    /// <returns> The type of startup that the application was launched with. </returns>
    public static StartupType GetLaunchType() {
        if (Args.Has("startup")) {
            return StartupType.SystemStartup;
        }

        if (Get()) {
            return StartupType.UserStartup;
        }

        return StartupType.Manual;
    }

    /// <summary> Gets whether the application is run with administrative privileges (or has the required registry permissions). </summary>
    /// <returns> <see langword="true"/> if the application is run with administrative privileges (or has the required registry permissions); otherwise, <see langword="false"/>. </returns>
    public static bool IsRunAsAdmin() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return IsRunAsAdmin_Windows();
        }

        return true;
    }

    static bool IsRunAsAdmin_Windows() {
        using RegistryKey? Key = Registry.CurrentUser.OpenSubKey(_WindowsRunKey, true);
        return Key is not null;
    }

    /// <summary> Restarts the application. </summary>
    /// <param name="Elevated"> Whether to restart the application with administrative privileges. </param>
    /// <param name="Args"> Additional command line arguments to pass to the restarted application. </param>
    /// <exception cref="NotSupportedException"> Thrown when the operating system is not supported. </exception>
    public static void Restart( bool Elevated = false, params string[] Args ) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Restart_Windows(Elevated, Args);
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            Restart_Mac(Args);
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            Restart_Linux(Args);
        } else {
            throw new NotSupportedException("Unsupported operating system.");
        }
    }

    static void Restart_Windows( bool Elevated, params string[] Args ) {
        string           ArgStr = string.Join(" ", Args);
        string           Path   = PathToApp;
        ProcessStartInfo Info;
        if (Elevated) {
            Path   = $"\"{Path}\"";
            ArgStr = $"\"{ArgStr}\"";
            Info = new() {
                FileName         = "cmd",
                Arguments        = $"/c start \"\" \"{Path}\" {ArgStr}",
                UseShellExecute  = true,
                CreateNoWindow   = true,
                WindowStyle      = ProcessWindowStyle.Hidden,
                Verb             = "runas",
                WorkingDirectory = Environment.CurrentDirectory,
                LoadUserProfile  = true,
                ErrorDialog      = true
            };
        } else {
            Info = new() {
                FileName         = Path,
                Arguments        = ArgStr,
                UseShellExecute  = true,
                CreateNoWindow   = true,
                WindowStyle      = ProcessWindowStyle.Hidden,
                WorkingDirectory = Environment.CurrentDirectory,
                LoadUserProfile  = true,
                ErrorDialog      = true
            };
        }

        Debug.WriteLine($"Restarting with command: {Info.FileName} {Info.Arguments}");
        Process.Start(Info);
        Environment.Exit(0);
    }

    static void Restart_Mac( params string[] Args ) {
        string ArgStr = string.Join(" ", Args);
        string Path   = PathToApp;
        ProcessStartInfo Info = new() {
            FileName         = "open",
            Arguments        = $"-n \"{Path}\" --args {ArgStr}",
            UseShellExecute  = true,
            CreateNoWindow   = true,
            WindowStyle      = ProcessWindowStyle.Hidden,
            WorkingDirectory = Environment.CurrentDirectory,
            LoadUserProfile  = true,
            ErrorDialog      = true
        };
        Debug.WriteLine($"Restarting with command: {Info.FileName} {Info.Arguments}");
        Process.Start(Info);
        Environment.Exit(0);
    }

    static void Restart_Linux( params string[] Args ) {
        string ArgStr = string.Join(" ", Args);
        string Path   = PathToApp;
        ProcessStartInfo Info = new() {
            FileName         = "xdg-open",
            Arguments        = $"\"{Path}\" {ArgStr}",
            UseShellExecute  = true,
            CreateNoWindow   = true,
            WindowStyle      = ProcessWindowStyle.Hidden,
            WorkingDirectory = Environment.CurrentDirectory,
            LoadUserProfile  = true,
            ErrorDialog      = true
        };
        Debug.WriteLine($"Restarting with command: {Info.FileName} {Info.Arguments}");
        Process.Start(Info);
        Environment.Exit(0);
    }
}

public enum StartupType {
    /// <summary> The application was started manually by the user. </summary>
    Manual,
    /// <summary> The application was started as part of user-defined startup. </summary>
    UserStartup,
    /// <summary> The application was started as part of system startup. </summary>
    SystemStartup
}
