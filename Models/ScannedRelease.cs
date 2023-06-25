namespace PlayRandom.Models;

public readonly struct ScannedRelease : IComparable<ScannedRelease>, IComparable, IEquatable<Version>, IEquatable<ScannedRelease> {
    /// <summary> The <see cref="Release"/> that was scanned. </summary>
    public readonly Release Release;

    /// <summary> The <see cref="Version"/> of the <see cref="Release"/>. </summary>
    public readonly Version Version;

    ScannedRelease( Release Release, Version Version ) {
        this.Release = Release;
        this.Version = Version;
    }

    /// <summary> The url to the <see cref="Release"/>. </summary>
    public Uri Uri => new(Release.HtmlUrl);

    /// <summary> The branch of the <see cref="Release"/>. </summary>
    public string Branch => Release.TargetCommitish;

    /// <summary> The url to the <see cref="Release"/>'s branch. </summary>
    public Uri BranchUri => new(Uri, "/tree/" + Branch);

    /// <summary> The url to the <see cref="Release"/>'s download. </summary>
    public Uri DownloadUri => new(Release.Assets[0].BrowserDownloadUrl);

    /// <summary> Attempts to create a <see cref="ScannedRelease"/> from a <see cref="Release"/>. </summary>
    /// <param name="Release"> The <see cref="Release"/> to create a <see cref="ScannedRelease"/> from. </param>
    /// <param name="ScannedRelease"> The created <see cref="ScannedRelease"/>. </param>
    /// <returns> <see langword="true"/> if the <see cref="ScannedRelease"/> was created successfully; otherwise, <see langword="false"/>. </returns>
    public static bool TryCreate( Release Release, [NotNullWhen(true)] out ScannedRelease? ScannedRelease ) {
        if (TryGetVersion(Release, out Version? Version)) {
            ScannedRelease = new(Release, Version);
            return true;
        }
        ScannedRelease = null;
        return false;
    }

    /// <summary> Scans the given collection of <see cref="Release"/>. </summary>
    /// <param name="Releases"> The <see cref="Release"/> to scan. </param>
    /// <returns> The scanned <see cref="ScannedRelease"/>(s), in order of version (from lowest to highest). </returns>
    public static IReadOnlyList<ScannedRelease> Scan( IReadOnlyCollection<Release> Releases ) {
        List<ScannedRelease> ScannedReleases = new(Releases.Count);
        foreach (Release Release in Releases) {
            if (TryCreate(Release, out ScannedRelease? ScannedRelease)) {
                ScannedReleases.Add(ScannedRelease.Value);
            }
        }

        ScannedReleases.Sort();
        return ScannedReleases;
    }

    #region Relational Members

    /// <inheritdoc />
    public int CompareTo( ScannedRelease Other ) => Version.CompareTo(Other.Version);

    /// <inheritdoc />
    public int CompareTo( object? Obj ) {
        if (ReferenceEquals(null, Obj)) {
            return 1;
        }

        return Obj is ScannedRelease Other ? CompareTo(Other) : throw new ArgumentException($"Object must be of type {nameof(ScannedRelease)}");
    }

    public static bool operator <( ScannedRelease  Left, ScannedRelease Right ) => Left.CompareTo(Right) < 0;
    public static bool operator >( ScannedRelease  Left, ScannedRelease Right ) => Left.CompareTo(Right) > 0;
    public static bool operator <=( ScannedRelease Left, ScannedRelease Right ) => Left.CompareTo(Right) <= 0;
    public static bool operator >=( ScannedRelease Left, ScannedRelease Right ) => Left.CompareTo(Right) >= 0;

    #endregion

    static bool TryGetVersion( Release Release, [NotNullWhen(true)] out Version? Version ) {
        if (Release.TagName is { } TagName) {
            if (Version.TryParse(TagName, out Version? ReleaseVersion)) {
                Version = ReleaseVersion;
                return true;
            }
        }
        Version = null;
        return false;
    }

    #region Equality Members

    /// <inheritdoc />
    public bool Equals( ScannedRelease Other ) => Version.Equals(Other.Version);

    /// <inheritdoc />
    public bool Equals( Version? Other ) => Version.Equals(Other);

    /// <inheritdoc />
    public override bool Equals( object? Obj ) => Obj switch {
        ScannedRelease Other => Equals(Other),
        Version Other        => Equals(Other),
        _                    => false
    };

    /// <inheritdoc />
    public override int GetHashCode() => Version.GetHashCode();

    public static bool operator ==( ScannedRelease Left, ScannedRelease Right ) => Left.Equals(Right);
    public static bool operator !=( ScannedRelease Left, ScannedRelease Right ) => !Left.Equals(Right);

    #endregion

}
