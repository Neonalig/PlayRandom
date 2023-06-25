using System.Globalization;

namespace PlayRandom.Converters;

[ValueConversion(typeof(string), typeof(bool))]
public sealed class UniqueStringToBooleanConverter : ValueConverter<string, bool>, IMultiValueConverter {

    /// <summary> The string comparison to use. </summary>
    public static readonly DependencyProperty ComparisonProperty = DependencyProperty.Register(nameof(Comparison), typeof(StringComparison), typeof(StringToVisibilityConverter), new(StringComparison.OrdinalIgnoreCase));

    /// <summary> Gets or sets the string comparison to use. </summary>
    public StringComparison Comparison {
        get => (StringComparison)GetValue(ComparisonProperty);
        set => SetValue(ComparisonProperty, value);
    }

    #region Overrides of ValueConverter<string,Visibility>

    /// <inheritdoc />
    protected override bool Convert( string Value, object? Parameter, CultureInfo Culture ) => string.Equals(Value, Parameter as string, Comparison);

    #endregion

    #region Implementation of IMultiValueConverter

    /// <inheritdoc />
    public object Convert( object[] Values, Type TargetType, object Parameter, CultureInfo Culture ) {
        if (Values.Length != 2) {
            throw new ArgumentException("There must be exactly two values.", nameof(Values));
        }

        if (Values[0] is not string A || Values[1] is not string B) {
            throw new ArgumentException("Both values must be strings.", nameof(Values));
        }

        return Convert(A, B, Culture);
    }

    /// <inheritdoc />
    public object[] ConvertBack( object Value, Type[] TargetTypes, object Parameter, CultureInfo Culture ) => throw new NotSupportedException();

    #endregion

}
