namespace Services.Interfaces;

/// <summary>
/// This interface defines a chat bot service.
/// </summary>
public interface IChatBotService
{
    Task ProcessMessageAsync(string phone, string message);
}
