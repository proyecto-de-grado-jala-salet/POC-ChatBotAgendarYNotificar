namespace Model;

/// <summary>
/// This class represents an entry in the webhook response.
/// </summary>
public class Entry
{
    public Change[] Changes { get; set; } = Array.Empty<Change>();
}