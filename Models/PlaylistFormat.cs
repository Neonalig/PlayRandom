using System.Diagnostics;
using Ardalis.SmartEnum;

namespace PlayRandom.Services;

public sealed class PlaylistFormat : SmartEnum<PlaylistFormat, string> {
    PlaylistFormat( string Name, string Extension ) : base(Name, Extension) {
        Debug.Assert(Extension.StartsWith('.'), "Extension must start with a period.");
        Debug.Assert(Extension == Extension.ToLower(), "Extension must be lowercase.");
    }

    /// <inheritdoc cref="SmartEnum{TEnum,TValue}.Value"/>
    public string Extension => Value;

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

}
