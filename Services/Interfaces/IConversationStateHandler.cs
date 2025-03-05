namespace Services.Interfaces;

/// <summary>
/// This interface handles messages based on conversation state.
/// </summary>
public interface IConversationStateHandler
{
    string State { get; }
    Task HandleMessageAsync(string phone, string message);
}
