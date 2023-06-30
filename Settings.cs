using PlayRandom.Models;
using PlayRandom.Views.Windows;

namespace PlayRandom;

public static class Settings {
    static readonly Dictionary<string, object> _Settings;

    const string _FilePath = "settings.json"; // path to settings file

    static readonly JsonSerializerOptions _Options = new() {
        IncludeFields = true,
        Converters = {
            new JsonStringEnumConverter(),
            new FileInfoConverter(),
            new DirectoryInfoConverter()
        }
    };

    static Settings() {
        _Settings = new();
        if (File.Exists(_FilePath)) {
            Load();
        }
    }

    /// <summary> Whether there are any changes to save. </summary>
    public static bool HasUnsavedChanges { get; private set; }

    /// <summary> Gets a setting. </summary>
    /// <param name="Name"> The name of the setting. </param>
    /// <param name="Fallback"> The fallback value. </param>
    /// <typeparam name="T"> The type of the setting. </typeparam>
    /// <returns> The setting. </returns>
    public static T Get<T>( string Name, T Fallback = default! ) where T : notnull {
        if (TryGet<T>(Name, out T? Value)) {
            return Value;
        }

        return Fallback;
    }

    /// <summary> Gets a setting. </summary>
    /// <param name="Name"> The name of the setting. </param>
    /// <param name="Fallback"> The fallback value. </param>
    /// <typeparam name="T"> The type of the setting. </typeparam>
    /// <returns> The setting. </returns>
    public static T Get<T>( string Name, Func<T> Fallback ) where T : notnull {
        if (TryGet<T>(Name, out T? Value)) {
            return Value;
        }

        return Fallback();
    }

    /// <summary> Attempts to get a setting. </summary>
    /// <param name="Name"> The name of the setting. </param>
    /// <param name="Value"> [out] The value of the setting. </param>
    /// <typeparam name="T"> The type of the setting. </typeparam>
    /// <returns> <see langword="true"/> if the setting was found; otherwise, <see langword="false"/>. </returns>
    public static bool TryGet<T>( string Name, [MaybeNullWhen(false)] out T Value ) where T : notnull {
        if (_Settings.TryGetValue(Name, out object? Object)) {
            if (Object is JsonElement Element) {
                Value = Element.ToObject<T>(_Options);
                return true;
            }

            Value = (T)Object;
            return true;
        }

        Value = default!;
        return false;
    }

    /// <summary> Gets a setting. </summary>
    /// <param name="Name"> The name of the setting. </param>
    /// <param name="Fallback"> The fallback value. </param>
    /// <param name="Token"> The cancellation token. </param>
    /// <typeparam name="T"> The type of the setting. </typeparam>
    /// <returns> The asynchronous operation which returns the setting. </returns>
    public static async Task<T> GetAsync<T>( string Name, T Fallback, CancellationToken Token = default ) where T : notnull {
        OneOf<T, None> Result = await TryGetAsync<T>(Name, Token);
        if (Result.TryPickT0(out T Value, out _)) {
            return Value;
        }

        return Fallback;
    }

    /// <summary> Attempts to get a setting. </summary>
    /// <param name="Name"> The name of the setting. </param>
    /// <param name="Token"> The cancellation token. </param>
    /// <typeparam name="T"> The type of the setting. </typeparam>
    /// <returns> The asynchronous operation which returns the value or none. </returns>
    public static async Task<OneOf<T, None>> TryGetAsync<T>( string Name, CancellationToken Token = default ) where T : notnull {
        if (_Settings.TryGetValue(Name, out object? Value)) {
            if (Value is JsonElement Element) {
                return await Element.ToObjectAsync<T>(_Options, Token);
            }

            return (T)Value;
        }

        return new None();
    }

    /// <summary> Sets a setting. </summary>
    /// <param name="Name"> The name of the setting. </param>
    /// <param name="Value"> The value of the setting. </param>
    /// <typeparam name="T"> The type of the setting. </typeparam>
    public static void Set<T>( string Name, T Value ) where T : notnull {
        _Settings[Name] = Value;
        HasUnsavedChanges      = true;
        Debug.WriteLine($"Set {Name} to {Value}");
        Changed?.Invoke(Name, Value);
    }

    static void Clear() => _Settings.Clear();
    static void AddRange( IEnumerable<KeyValuePair<string, object>> Collection ) {
        foreach (KeyValuePair<string, object> Pair in Collection) {
            _Settings[Pair.Key] = Pair.Value;
        }
    }

    /// <summary> Loads the settings from the settings file. </summary>
    /// <param name="Token"> The cancellation token. </param>
    /// <returns> The asynchronous operation. </returns>
    static async Task LoadAsync( CancellationToken Token = default ) {
        if (!File.Exists(_FilePath)) { return; }

        await using FileStream Stream = File.OpenRead(_FilePath);
        Clear();
        Dictionary<string, object>? ToAdd = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(Stream, cancellationToken: Token, options: _Options);
        if (ToAdd is null) {
            throw new InvalidOperationException($"Could not deserialise {Stream} to {typeof(Dictionary<string, object>)}");
        }

        AddRange(ToAdd);
        HasUnsavedChanges = false;
    }

    /// <summary> Loads the settings from the settings file. </summary>
    static void Load() {
        if (!File.Exists(_FilePath)) { return; }

        string Json = File.ReadAllText(_FilePath);
        Clear();
        Dictionary<string, object>? ToAdd = JsonSerializer.Deserialize<Dictionary<string, object>>(Json, _Options);
        if (ToAdd is null) {
            throw new InvalidOperationException($"Could not deserialise {Json} to {typeof(Dictionary<string, object>)}");
        }

        AddRange(ToAdd);
        HasUnsavedChanges = false;
    }

    /// <summary> Saves the settings to the settings file. </summary>
    /// <param name="Token"> The cancellation token. </param>
    /// <returns> The asynchronous operation. </returns>
    public static async Task SaveAsync( CancellationToken Token = default ) {
        await using FileStream Stream = new(_FilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
        await JsonSerializer.SerializeAsync(Stream, _Settings, cancellationToken: Token, options: _Options);
        HasUnsavedChanges = false;
    }

    /// <summary> Saves the settings to the settings file. </summary>
    public static void Save() {
        string Json = JsonSerializer.Serialize(_Settings, _Options);
        File.WriteAllText(_FilePath, Json);
        HasUnsavedChanges = false;
    }

    /// <summary> Reverts the settings to the last saved state. </summary>
    /// <param name="Token"> The cancellation token. </param>
    /// <returns> The asynchronous operation. </returns>
    public static async Task RevertAsync( CancellationToken Token = default ) => await LoadAsync(Token);

    /// <summary> Clears all settings and deletes the settings file. </summary>
    /// <param name="Token"> The cancellation token. </param>
    /// <returns> The asynchronous operation. </returns>
    public static async Task ClearAsync( CancellationToken Token = default ) {
        Clear();
        if (File.Exists(_FilePath)) {
            File.Delete(_FilePath);
        }

        await SaveAsync(Token);
    }

    /// <summary> Whether there are any settings, saved or otherwise. </summary>
    public static bool Any => _Settings.Count != 0;

    #region Hard-coded
    readonly struct Setting<T> where T : notnull {
        readonly string            _Name;
        readonly OneOf<T, Func<T>> _Default;

        public Setting( string Name, T Default ) {
            _Name    = Name;
            _Default = Default;
        }
        public Setting( string Name, Func<T> Default ) {
            _Name    = Name;
            _Default = Default;
        }

        /// <inheritdoc cref="Settings.Get{T}(string,T)"/>
        public T Get() => _Default.TryPickT0(out T Default, out Func<T> Factory) ? Settings.Get(_Name, Default) : Settings.Get(_Name, Factory());

        /// <inheritdoc cref="Settings.GetAsync{T}(string,T,CancellationToken)"/>
        public Task<T> GetAsync( CancellationToken Token = default ) => _Default.TryPickT0(out T Default, out Func<T> Factory) ? Settings.GetAsync(_Name, Default, Token) : Settings.GetAsync(_Name, Factory(), Token);

        /// <inheritdoc cref="Settings.Set{T}(string,T)"/>
        public void Set( T Value ) => Settings.Set(_Name, Value);
    }

    static readonly Setting<ExecutableLauncher> _Executable = new(nameof(Executable), ExecutableLauncher.GetSystemDefault(".m3u"));
    /// <summary> Gets or sets the executable to launch. </summary>
    public static ExecutableLauncher Executable {
        get => _Executable.Get();
        set => _Executable.Set(value);
    }

    static readonly Setting<bool> _OfferToRememberSearchPath = new(nameof(OfferToRememberSearchPath), true);
    /// <summary> Gets or sets whether to offer to remember the search path. </summary>
    public static bool OfferToRememberSearchPath {
        get => _OfferToRememberSearchPath.Get();
        set => _OfferToRememberSearchPath.Set(value);
    }

    static readonly Setting<string> _LastSearchPath = new(nameof(LastSearchPath), Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
    /// <summary> Gets or sets the last saved search path. </summary>
    public static string LastSearchPath {
        get => _LastSearchPath.Get();
        set => _LastSearchPath.Set(value);
    }

    /// <summary> Gets or sets whether to start with the operating system. </summary>
    public static bool StartWithOS {
        get => Startup.Get();
        set {
            if (value == Startup.Get()) { return; }
            Startup.Set(value);
        }
    }

    static readonly Setting<bool> _MinimiseToTray = new(nameof(MinimiseToTray), false);
    /// <summary> Gets or sets whether to minimise to the system tray. </summary>
    public static bool MinimiseToTray {
        get => _MinimiseToTray.Get();
        set => _MinimiseToTray.Set(value);
    }

    static readonly Setting<bool> _CloseToTray = new(nameof(CloseToTray), false);
    /// <summary> Gets or sets whether to close to the system tray. </summary>
    public static bool CloseToTray {
        get => _CloseToTray.Get();
        set => _CloseToTray.Set(value);
    }

    static readonly Setting<bool> _AlwaysOnTop = new(nameof(AlwaysOnTop), false);
    /// <summary> Gets or sets whether the window should always be on top. </summary>
    public static bool AlwaysOnTop {
        get => _AlwaysOnTop.Get();
        set {
            _AlwaysOnTop.Set(value);
            if (Application.Current.MainWindow is MainWindow MW) {
                MW.Topmost = value;
            }
        }
    }

    static readonly Setting<bool> _UseMediaKeys = new(nameof(UseMediaKeys), false);
    /// <summary> Gets or sets whether to use media keys for shuffling and playing. </summary>
    public static bool UseMediaKeys {
        get => _UseMediaKeys.Get();
        set => _UseMediaKeys.Set(value);
    }

    #endregion

    public delegate void SettingChangedEventHandler( string Name, object Value );

    /// <summary> Raised when the settings have changed. </summary>
    public static event SettingChangedEventHandler? Changed;
}

public static class JsonElementExtensions {
    /// <summary> Deserialises a <see cref="JsonElement"/> to an object. </summary>
    /// <param name="Element"> The <see cref="JsonElement"/> to deserialise. </param>
    /// <param name="Options"> The <see cref="JsonSerializerOptions"/> to use. </param>
    /// <param name="Token"> The cancellation token. </param>
    /// <typeparam name="T"> The type of the object. </typeparam>
    /// <returns> The asynchronous operation which returns the deserialised object. </returns>
    /// <exception cref="InvalidOperationException"> Thrown when the <see cref="JsonElement"/> could not be deserialised. </exception>
    public static async Task<T> ToObjectAsync<T>( this JsonElement Element, JsonSerializerOptions? Options = null, CancellationToken Token = default ) where T : notnull {
        string             Json         = Element.GetRawText();
        byte[]             Bytes        = Encoding.UTF8.GetBytes(Json);
        using MemoryStream MemoryStream = new(Bytes);
        T?                 Result       = await JsonSerializer.DeserializeAsync<T>(MemoryStream, cancellationToken: Token, options: Options);
        if (Result is null) {
            throw new InvalidOperationException($"Could not deserialise {Json} to {typeof(T)}");
        }

        return Result;
    }

    /// <summary> Deserialises a <see cref="JsonElement"/> to an object. </summary>
    /// <param name="Element"> The <see cref="JsonElement"/> to deserialise. </param>
    /// <param name="Options"> The <see cref="JsonSerializerOptions"/> to use. </param>
    /// <typeparam name="T"> The type of the object. </typeparam>
    /// <returns> The deserialised object. </returns>
    /// <exception cref="InvalidOperationException"> Thrown when the <see cref="JsonElement"/> could not be deserialised. </exception>
    public static T ToObject<T>( this JsonElement Element, JsonSerializerOptions? Options = null ) where T : notnull {
        string Json   = Element.GetRawText();
        T?     Result = JsonSerializer.Deserialize<T>(Json, Options);
        if (Result is null) {
            throw new InvalidOperationException($"Could not deserialise {Json} to {typeof(T)}");
        }
        return Result;
    }
}

public sealed class FileInfoConverter : JsonConverter<FileInfo> {
    /// <inheritdoc />
    public override FileInfo Read( ref Utf8JsonReader Reader, Type TypeToConvert, JsonSerializerOptions Options ) {
        string? Path = Reader.GetString();
        if (Path is null) {
            throw new JsonException("Expected string");
        }
        if (!Path.TryGetFileInfo(out FileInfo? File)) {
            throw new JsonException($"Invalid file path '{Path}'");
        }
        return File;
    }

    /// <inheritdoc />
    public override void Write( Utf8JsonWriter Writer, FileInfo Value, JsonSerializerOptions Options ) {
        Writer.WriteStringValue(Value.FullName);
    }
}

public sealed class DirectoryInfoConverter : JsonConverter<DirectoryInfo> {
    /// <inheritdoc />
    public override DirectoryInfo Read( ref Utf8JsonReader Reader, Type TypeToConvert, JsonSerializerOptions Options ) {
        string? Path = Reader.GetString();
        if (Path is null) {
            throw new JsonException("Expected string");
        }
        if (!Path.TryGetDirectoryInfo(out DirectoryInfo? Directory)) {
            throw new JsonException($"Invalid directory path '{Path}'");
        }
        return Directory;
    }

    /// <inheritdoc />
    public override void Write( Utf8JsonWriter Writer, DirectoryInfo Value, JsonSerializerOptions Options ) {
        Writer.WriteStringValue(Value.FullName);
    }
}
