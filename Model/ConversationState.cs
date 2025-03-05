namespace Model;

/// <summary>
/// This class stores the conversation state of a user.
/// </summary>
public class ConversationState
{
    public string State { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public DateTime? AppointmentDateTime { get; set; } = null;
}
