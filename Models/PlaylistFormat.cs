namespace PlayRandom.Services;

[DebuggerDisplay("{" + nameof(Name) + "}")]
public readonly struct PlaylistFormat : IEquatable<PlaylistFormat>, IComparable<PlaylistFormat>, IComparable {
    static readonly HashSet<PlaylistFormat> _Values = new();

    /// <summary> The name of the playlist format. </summary>
    public readonly string Name;

    /// <summary> The extension of the playlist format. </summary>
    public readonly string Extension;

    readonly int _Index;

    PlaylistFormat( string Name, string Extension ) {
        Debug.Assert(Extension.StartsWith('.'), "Extension must start with a period.");
        Debug.Assert(Extension == Extension.ToLower(), "Extension must be lowercase.");

        this.Name      = Name;
        this.Extension = Extension;
        _Index         = _Values.Count;

        _Values.Add(this);
    }

    /// <summary> All playlist formats. </summary>
    public static IReadOnlyCollection<PlaylistFormat> Values => _Values;

    /// <summary> Advanced Stream Redirector. </summary>
    public static readonly PlaylistFormat ASX = new(nameof(ASX), ".asx");

    /// <summary> Cue Sheet. </summary>
    public static readonly PlaylistFormat CUE = new(nameof(CUE), ".cue");

    /// <summary> MP3 Url. </summary>
    public static readonly PlaylistFormat M3U = new(nameof(M3U), ".m3u");

    /// <summary> MP3 Url UTF-8. </summary>
    public static readonly PlaylistFormat M3U8 = new(nameof(M3U8), ".m3u8");

    /// <summary> Nullsoft Playlist. </summary>
    public static readonly PlaylistFormat PLS = new(nameof(PLS), ".pls");

    /// <summary> Synchronised Accessible Media Interchange. </summary>
    public static readonly PlaylistFormat SAMI = new(nameof(SAMI), ".sami");

    /// <summary> Windows Media Player Playlist. </summary>
    public static readonly PlaylistFormat WPL = new(nameof(WPL), ".wpl");

    /// <summary> Winamp Playlist. </summary>
    public static readonly PlaylistFormat WAX = new(nameof(WAX), ".wax");

    /// <summary> XML Shareable Playlist Format. </summary>
    public static readonly PlaylistFormat XSPF = new(nameof(XSPF), ".xspf");

    /// <summary> Zune Playlist. </summary>
    public static readonly PlaylistFormat ZPL = new(nameof(ZPL), ".zpl");

    #region Overrides of ValueType

    /// <inheritdoc />
    public override string ToString() => Name;

    #endregion

    #region Equality Members

    /// <inheritdoc />
    public bool Equals( PlaylistFormat Other ) => _Index == Other._Index;

    /// <inheritdoc />
    public override bool Equals( object? Obj ) => Obj is PlaylistFormat Other && Equals(Other);

    /// <inheritdoc />
    public override int GetHashCode() => _Index;

    public static bool operator ==( PlaylistFormat Left, PlaylistFormat Right ) => Left.Equals(Right);
    public static bool operator !=( PlaylistFormat Left, PlaylistFormat Right ) => !Left.Equals(Right);

    #endregion

    #region Relational Members

    /// <inheritdoc />
    public int CompareTo( PlaylistFormat Other ) => _Index.CompareTo(Other._Index);

    /// <inheritdoc />
    public int CompareTo( object? Obj ) {
        if (ReferenceEquals(null, Obj)) {
            return 1;
        }

        return Obj is PlaylistFormat Other ? CompareTo(Other) : throw new ArgumentException($"Object must be of type {nameof(PlaylistFormat)}");
    }

    public static bool operator <( PlaylistFormat  Left, PlaylistFormat Right ) => Left.CompareTo(Right) < 0;
    public static bool operator >( PlaylistFormat  Left, PlaylistFormat Right ) => Left.CompareTo(Right) > 0;
    public static bool operator <=( PlaylistFormat Left, PlaylistFormat Right ) => Left.CompareTo(Right) <= 0;
    public static bool operator >=( PlaylistFormat Left, PlaylistFormat Right ) => Left.CompareTo(Right) >= 0;

    #endregion

}
