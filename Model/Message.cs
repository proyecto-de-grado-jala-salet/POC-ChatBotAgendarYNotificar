namespace Model;

/// <summary>
/// This class represents a message.
/// </summary>
public class Message
{
    public required string Id { get; set; }
    public required string From { get; set; }
    public required string Type { get; set; }
    public required Text Text { get; set; }
}
