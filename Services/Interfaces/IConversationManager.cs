using Model;

namespace Services.Interfaces;

/// <summary>
/// This interface manages the conversation state of users.
/// </summary>
public interface IConversationManager
{
    ConversationState GetState(string phone);
    void SetState(string phone, string state);
    void SetSpecialty(string phone, string specialty);
    string GetSpecialty(string phone);
    void ClearState(string phone);
}
