namespace PlayRandom;

public readonly struct ArgDef {
    /// <summary> The name of the argument. </summary>
    public readonly string Name = string.Empty;

    /// <summary> The short name of the argument. </summary>
    public readonly char ShortName = '\0';

    /// <summary> Creates a new argument definition. </summary>
    /// <param name="Name"> The name of the argument. </param>
    /// <param name="ShortName"> The short name of the argument. </param>
    public ArgDef( string Name, char ShortName = '\0' ) {
        this.Name      = Name;
        this.ShortName = ShortName;
    }

    /// <inheritdoc cref="ArgDef(string, char)"/>
    public ArgDef( char ShortName ) : this(string.Empty, ShortName) { }

    /// <summary> An empty argument definition. </summary>
    public static readonly ArgDef None = new();

    public static implicit operator ArgDef( string Name ) {
        switch (Name.Length) {
            case 0:
                return None;
            case 1:
                return new(Name[0]);
            case 2 when Name[0] == '-':
                return Name[1] switch {
                    '-' => new(Name[2..]),
                    _   => new(Name[1]),
                };
            default:
                if (Name[0] == '-' && Name[1] == '-') {
                    return new(Name[2..]);
                }
                if (Name[0] == '-') {
                    Debug.Fail("Invalid argument name. Expected '--' for long name, but got '-' instead.");
                }
                return new(Name);
        }
    }
    public static implicit operator ArgDef( char ShortName ) => ShortName is '-' or '\0' ? None : new(ShortName);

    /// <summary> Checks if the given argument matches this definition. </summary>
    /// <param name="Other"> The argument to check. </param>
    /// <param name="Comparison"> The string comparison to use. </param>
    /// <returns> <see langword="true"/> if the argument matches this definition, otherwise <see langword="false"/>. </returns>
    public bool Matches( ArgDef Other, StringComparison Comparison = StringComparison.Ordinal ) => Name.Equals(Other.Name, Comparison) || AreEqual(ShortName, Other.ShortName, Comparison);

    /// <summary> Checks if the given characters are equal. </summary>
    /// <param name="A"> The first character. </param>
    /// <param name="B"> The second character. </param>
    /// <param name="Comparison"> The string comparison to use. </param>
    /// <returns> <see langword="true"/> if the characters are equal, otherwise <see langword="false"/>. </returns>
    static bool AreEqual(char A, char B, StringComparison Comparison) {
        switch (Comparison) {
            case StringComparison.Ordinal:
            case StringComparison.InvariantCulture:
                return A == B;

            case StringComparison.OrdinalIgnoreCase:
            case StringComparison.InvariantCultureIgnoreCase:
                return char.ToUpperInvariant(A) == char.ToUpperInvariant(B);

            default:
                return string.Equals(A.ToString(), B.ToString(), Comparison);
        }
    }

    #region Overrides of ValueType

    /// <inheritdoc />
    public override string ToString() => Name;

    #endregion

}

public readonly struct Arg {
    /// <summary> The definition of the argument. </summary>
    public readonly ArgDef Def;

    /// <summary> The value of the argument. </summary>
    public readonly string Value;

    /// <summary> Creates a new parsed argument. </summary>
    /// <param name="Def"> The definition of the argument. </param>
    /// <param name="Value"> The value of the argument. </param>
    public Arg( ArgDef Def, string Value ) {
        this.Def   = Def;
        this.Value = Value;
    }

    /// <inheritdoc cref="ArgDef.Matches(ArgDef, StringComparison)"/>
    public bool Matches( ArgDef Other, StringComparison Comparison = StringComparison.Ordinal ) => Def.Matches(Other, Comparison);

    #region Overrides of ValueType

    /// <inheritdoc />
    public override string ToString() => $"{Def.Name}={Value}";

    #endregion

}

public static class Args {
    // Unix style arguments
    //  Example: 'executable.exe --arg1 "test" -bc 7'
    //  Becomes: '[{"arg1", "test"}, {"b", 7}, {"c", 7}]'

    /// <summary> Parses the given argument string. </summary>
    /// <param name="ArgString"> The argument string to parse. </param>
    /// <returns> The parsed arguments. </returns>
    public static IReadOnlyList<Arg> Parse( string ArgString ) {
        Debug.WriteLine($"Parsing arguments: {ArgString}");
        List<Arg> Args = new();

        StringBuilder SB    = new();
        int           Stage = 0;     // Stage 0: Parsing name, Stage 1: Parsing value
        bool          Multi = false; // '-abc test' -> '-a test' '-b test' '-c test'
        ArgDef        Def   = ArgDef.None;

        bool EscapeNextChar = false;

        foreach (char C in ArgString) {
            if (EscapeNextChar) {
                SB.Append(C);
                EscapeNextChar = false;
                continue;
            }

            switch (C) {
                case '\\':
                    EscapeNextChar = true;
                    break;
                case '-':
                    switch (Stage) {
                        case 0 when SB.Length > 0: SB.Append(C);
                            break;
                        case 1:
                            AddArg();
                            SB.Clear();
                            break;
                    }

                    break;
                case ' ':
                    switch (Stage) {
                        case 0: {
                            Def = SB.ToString();
                            if (Def.ShortName != '\0' && Def.Name.Length == 0) {
                                Multi = true;
                            }

                            SB.Clear();
                            Stage = 1;
                            break;
                        }
                        case 1 when !string.IsNullOrWhiteSpace(SB.ToString()): AddArg();
                            break;
                    }

                    break;
                case '\"':
                    if (Stage == 1) {
                        AddArg();
                    }

                    break;
                default:
                    SB.Append(C);
                    break;
            }
        }

        if (Stage == 1 && !string.IsNullOrWhiteSpace(SB.ToString())) {
            AddArg();
        }

        Debug.WriteLine($"Parsed arguments: '{string.Join("', '", Args)}'");
        return Args;

        void AddArg() {
            string Value = SB.ToString();
            SB.Clear();
            if (Multi) {
                foreach (char Ch in Def.Name) {
                    Args.Add(new(new(Ch), Value));
                }

                Multi = false;
            } else {
                Args.Add(new(Def, Value));
            }

            Stage = 0;
            Def   = ArgDef.None;
        }
    }

    /// <inheritdoc cref="Parse(string)"/>
    public static IReadOnlyList<Arg> Parse( IEnumerable<string> Args ) => Parse(string.Join(' ', Args));

    static readonly Lazy<IReadOnlyList<Arg>> _SelfParsedArgs = new(() => Parse(Environment.GetCommandLineArgs().Skip(1)));

    /// <summary> The arguments passed to the program. </summary>
    public static IReadOnlyList<Arg> All => _SelfParsedArgs.Value;

    /// <summary> Attempts to get the value of the argument with the given name. </summary>
    /// <param name="Def"> The argument definition to look for. </param>
    /// <param name="Value"> The value of the argument. </param>
    /// <param name="Comparison"> The comparison to use when matching the name. </param>
    /// <returns> Whether the argument was found. </returns>
    public static bool TryGetValue( ArgDef Def, out string Value, StringComparison Comparison = StringComparison.Ordinal ) {
        foreach (Arg Arg in All) {
            if (Arg.Matches(Def, Comparison)) {
                Value = Arg.Value;
                return true;
            }
        }

        Value = string.Empty;
        return false;
    }

    static OneOf<string, char> GetTrueName( string Name ) {
        // Allows: "arg", "--arg", "a", "-a"
        switch (Name.Length) {
            case 1:
                return Name[0];
            case 2 when Name[0] == '-':
                return Name[1];
            default:
                if (Name.StartsWith("--")) {
                    return Name[2..];
                }

                Debug.Assert(!Name.StartsWith('-'), "Multiple short names are not supported.");
                return Name;
        }
    }

    /// <inheritdoc cref="TryGetValue(ArgDef, out string, StringComparison)"/>
    public static bool TryGetValue( string Name, out string Value, StringComparison Comparison = StringComparison.Ordinal ) => TryGetValue(Def: Name, out Value, Comparison);

    /// <inheritdoc cref="TryGetValue(ArgDef, out string, StringComparison)"/>
    public static bool TryGetValue( char ShortName, out string Value, StringComparison Comparison = StringComparison.Ordinal ) => TryGetValue(Def: ShortName, out Value, Comparison);

    /// <summary> Gets whether the argument with the given name exists. </summary>
    /// <param name="Def"> The argument definition to look for. </param>
    /// <param name="Comparison"> The comparison to use when matching the name. </param>
    /// <returns> <see langword="true"/> if the argument exists; otherwise, <see langword="false"/>. </returns>
    public static bool Has( ArgDef Def, StringComparison Comparison = StringComparison.Ordinal ) {
        foreach (Arg Arg in All) {
            if (Arg.Matches(Def, Comparison)) {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc cref="Has(ArgDef, StringComparison)"/>
    public static bool Has( string Name, StringComparison Comparison = StringComparison.Ordinal ) => Has(Def: Name, Comparison);

    /// <inheritdoc cref="Has(ArgDef, StringComparison)"/>
    public static bool Has( char ShortName, StringComparison Comparison = StringComparison.Ordinal ) => Has(Def: ShortName, Comparison);
}
