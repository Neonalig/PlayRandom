namespace PlayRandom.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BooleanToVisibilityConverter : ValueConverter<bool, Visibility> {

    /// <summary> The visibility to return when <see langword="false"/>. </summary>
    public static readonly DependencyProperty FalseProperty = DependencyProperty.Register(nameof(False), typeof(Visibility), typeof(BooleanToVisibilityConverter), new(Visibility.Collapsed));

    /// <summary> Gets or sets the visibility to return when <see langword="false"/>. </summary>
    public Visibility False {
        get => (Visibility)GetValue(FalseProperty);
        set => SetValue(FalseProperty, value);
    }

    /// <summary> The visibility to return when <see langword="true"/>. </summary>
    public static readonly DependencyProperty TrueProperty = DependencyProperty.Register(nameof(True), typeof(Visibility), typeof(BooleanToVisibilityConverter), new(Visibility.Visible));

    /// <summary> Gets or sets the visibility to return when <see langword="true"/>. </summary>
    public Visibility True {
        get => (Visibility)GetValue(TrueProperty);
        set => SetValue(TrueProperty, value);
    }

    #region Overrides of ValueConverter<bool,Visibility>

    /// <inheritdoc />
    public override Visibility Convert( bool Value, object? Parameter, CultureInfo Culture ) => Value ? True : False;

    /// <inheritdoc />
    public override bool ConvertBack( Visibility Value, object? Parameter, CultureInfo Culture ) => Value == True;

    #endregion

}
