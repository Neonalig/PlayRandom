using System.Globalization;

namespace PlayRandom.Converters;

[ValueConversion(typeof(string), typeof(Visibility))]
public sealed class StringToVisibilityConverter : ValueConverter<string, Visibility> {

    /// <summary> The visibility when the string is null or empty. </summary>
    public static readonly DependencyProperty EmptyProperty = DependencyProperty.Register(nameof(Empty), typeof(Visibility), typeof(StringToVisibilityConverter), new(Visibility.Collapsed));

    /// <summary> Gets or sets the visibility when the string is null or empty. </summary>
    public Visibility Empty {
        get => (Visibility)GetValue(EmptyProperty);
        set => SetValue(EmptyProperty, value);
    }

    /// <summary> The visibility when the string has a value. </summary>
    public static readonly DependencyProperty NotEmptyProperty = DependencyProperty.Register(nameof(NotEmpty), typeof(Visibility), typeof(StringToVisibilityConverter), new(Visibility.Visible));

    /// <summary> Gets or sets the visibility when the string has a value. </summary>
    public Visibility NotEmpty {
        get => (Visibility)GetValue(NotEmptyProperty);
        set => SetValue(NotEmptyProperty, value);
    }

    /// <summary> Whether to include whitespace when checking if the string is empty. </summary>
    public static readonly DependencyProperty IncludeWhitespaceProperty = DependencyProperty.Register(nameof(IncludeWhitespace), typeof(bool), typeof(StringToVisibilityConverter), new(false));

    /// <summary> Gets or sets whether to include whitespace when checking if the string is empty. </summary>
    public bool IncludeWhitespace {
        get => (bool)GetValue(IncludeWhitespaceProperty);
        set => SetValue(IncludeWhitespaceProperty, value);
    }

    #region Overrides of ValueConverter<string,Visibility>

    /// <inheritdoc />
    protected override Visibility Convert( string Value, object? Parameter, CultureInfo Culture ) {
        if (string.IsNullOrEmpty(Value) || (IncludeWhitespace && string.IsNullOrWhiteSpace(Value))) {
            return Empty;
        }

        return NotEmpty;
    }

    #endregion

}
