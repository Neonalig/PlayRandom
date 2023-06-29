namespace PlayRandom.Converters;

public sealed class PlatformToStringConverter : ValueConverter<PlatformID, string>, IStaticValueConverter<PlatformID, string> {

    /// <summary> The string representation of the current operating system. </summary>
    public static string String { get; } = StaticConvert(Environment.OSVersion.Platform, null, CultureInfo.CurrentCulture);

    #region Implementation of IStaticValueConverter<PlatformID,string>

    /// <inheritdoc />
    public static string StaticConvert( PlatformID Value, object? Parameter, CultureInfo Culture ) => Value switch {
        PlatformID.Win32S       => "Windows",
        PlatformID.Win32Windows => "Windows",
        PlatformID.Win32NT      => "Windows",
        PlatformID.WinCE        => "Windows",
        PlatformID.Unix         => "Unix",
        PlatformID.Xbox         => "Xbox",
        PlatformID.MacOSX       => "MacOS X",
        PlatformID.Other        => "the current operating system",
        _                       => throw new ArgumentOutOfRangeException(nameof(Value), Value, null)
    };

    /// <inheritdoc />
    public static PlatformID StaticConvertBack( string Value, object? Parameter, CultureInfo Culture ) => throw new NotSupportedException();

    #endregion

    #region Overrides of ValueConverter<PlatformID,string>

    /// <inheritdoc />
    public override string Convert( PlatformID Value, object? Parameter, CultureInfo Culture ) => StaticConvert(Value, Parameter, Culture);

    #endregion

}
