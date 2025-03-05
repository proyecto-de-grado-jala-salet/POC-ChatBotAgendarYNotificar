using System.Collections.Concurrent;
using Model;
using Services.Interfaces;

namespace Services;

/// <summary>
/// This class manages the conversation state for users.
/// It stores the state in memory.
/// </summary>
public class ConversationManager : IConversationManager
{
    private readonly ConcurrentDictionary<string, ConversationState> _conversations = new();

    /// <summary>
    /// Gets the conversation state for a user.
    /// </summary>
    /// <param name="phone">The user's phone number.</param>
    /// <returns>The conversation state.</returns>
    public ConversationState GetState(string phone)
    {
        return _conversations.GetOrAdd(phone, new ConversationState());
    }

    /// <summary>
    /// Sets a new state for a user.
    /// </summary>
    /// <param name="phone">The user's phone number.</param>
    /// <param name="state">The state to set.</param>
    public void SetState(string phone, string state)
    {
        var conv = GetState(phone);
        conv.State = state;
    }

    /// <summary>
    /// Sets the specialty for a user.
    /// </summary>
    /// <param name="phone">The user's phone number.</param>
    /// <param name="specialty">The specialty to set.</param>
    public void SetSpecialty(string phone, string specialty)
    {
        var conv = GetState(phone);
        conv.Specialty = specialty;
    }

    /// <summary>
    /// Gets the specialty for a user.
    /// </summary>
    /// <param name="phone">The user's phone number.</param>
    /// <returns>The specialty.</returns>
    public string GetSpecialty(string phone)
    {
        return GetState(phone).Specialty ?? string.Empty;
    }

    /// <summary>
    /// Clears the conversation state for a user.
    /// </summary>
    /// <param name="phone">The user's phone number.</param>
    public void ClearState(string phone)
    {
        _conversations.TryRemove(phone, out _);
    }
}
