namespace Services.Interfaces;

/// <summary>
/// This interface defines a service to send WhatsApp messages.
/// </summary>
public interface IWhatsAppService
{
    Task SendTextMessageAsync(string phone, string message);
}
