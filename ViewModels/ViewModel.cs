namespace PlayRandom.ViewModels;

public abstract class ViewModel : NotifyPropertyChanged { }

public abstract class NotifyPropertyChanged : DependencyObject, INotifyPropertyChanged {

    #region Implementation of INotifyPropertyChanged

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary> Raises the <see cref="PropertyChanged"/> event. </summary>
    /// <param name="PropertyName"> The name of the property that changed. </param>
    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged( [CallerMemberName] string? PropertyName = null ) => PropertyChanged?.Invoke(this, new(PropertyName));

    /// <summary> Sets the value of a field and raises the <see cref="PropertyChanged"/> event if the value changed. </summary>
    /// <param name="Field"> The field to set. </param>
    /// <param name="Value"> The value to set the field to. </param>
    /// <param name="PropertyName"> The name of the property that changed. </param>
    /// <typeparam name="T"> The type of the field. </typeparam>
    /// <returns> <see langword="true"/> if the value changed; otherwise, <see langword="false"/>. </returns>
    protected bool SetField<T>( ref T Field, T Value, [CallerMemberName] string? PropertyName = null ) {
        if (EqualityComparer<T>.Default.Equals(Field, Value)) {
            return false;
        }

        Field = Value;
        OnPropertyChanged(PropertyName);
        return true;
    }

    #endregion
}
