using PlayRandom.ViewModels;

namespace PlayRandom.Converters;

public interface IValueConverter<TIn, TOut> : IValueConverter {
    /// <inheritdoc cref="IValueConverter.Convert"/>
    TOut Convert( TIn Value, object? Parameter, CultureInfo Culture );

    /// <inheritdoc cref="IValueConverter.ConvertBack"/>
    TIn ConvertBack( TOut Value, object? Parameter, CultureInfo Culture );
}

public abstract class ValueConverter<TIn, TOut> : NotifyPropertyChanged, IValueConverter<TIn, TOut> {

    #region Implementation of IValueConverter

    /// <inheritdoc />
    object? IValueConverter.Convert( object Value, Type TargetType, object? Parameter, CultureInfo Culture ) => Value is TIn In ? Convert(In, Parameter, Culture) : throw new ArgumentException($"Value must be of type {typeof(TIn)}", nameof(Value));

    /// <inheritdoc />
    object? IValueConverter.ConvertBack( object Value, Type TargetType, object? Parameter, CultureInfo Culture ) => Value is TOut Out ? ConvertBack(Out, Parameter, Culture) : throw new ArgumentException($"Value must be of type {typeof(TOut)}", nameof(Value));

    #endregion

    #region Implementation of IValueConverter<TIn,TOut>

    /// <inheritdoc />
    public abstract TOut Convert( TIn Value, object? Parameter, CultureInfo Culture );

    /// <inheritdoc />
    public virtual TIn ConvertBack( TOut Value, object? Parameter, CultureInfo Culture ) => throw new NotSupportedException();

    #endregion

}

public interface IStaticValueConverter<TIn, TOut> : IValueConverter<TIn, TOut> {

    /// <inheritdoc cref="IValueConverter.Convert"/>
    public static abstract TOut StaticConvert( TIn Value, object? Parameter, CultureInfo Culture );

    /// <inheritdoc cref="IValueConverter.ConvertBack"/>
    public static abstract TIn StaticConvertBack( TOut Value, object? Parameter, CultureInfo Culture );

}
