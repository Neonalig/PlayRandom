namespace PlayRandom;

public static class UriExtensions {
    /// <summary> Opens the URL in the default browser. </summary>
    /// <param name="URL"> The URL to open. </param>
    public static void OpenUrl( this string URL ) {
        if (string.IsNullOrEmpty(URL)) { return; }
        try {
            ProcessStartInfo StartInfo = new() {
                FileName        = URL,
                UseShellExecute = true
            };
            Process.Start(StartInfo);
        } catch {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                URL = URL.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(URL) { UseShellExecute = true });
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Process.Start("xdg-open", URL);
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Process.Start("open", URL);
            } else {
                throw;
            }
        }
    }

    /// <inheritdoc cref="OpenUrl(string)"/>
    public static void OpenUrl( this Uri URL ) => URL.ToString().OpenUrl();
}
