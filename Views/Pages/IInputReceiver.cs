namespace PlayRandom.Views.Pages;

public interface IInputReceiver {
    /// <summary> Called when a key input is received. </summary>
    /// <param name="Sender"> The sender. </param>
    /// <param name="E"> The event arguments. </param>
    void OnKeyReceived( object Sender, KeyEventArgs E );
}
