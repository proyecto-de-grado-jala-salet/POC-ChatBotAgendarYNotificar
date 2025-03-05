namespace Model;

/// <summary>
/// This class represents the complete response from the webhook.
/// </summary>
public class WebHookResponseModel
{
    public Entry[] Entry { get; set; } = Array.Empty<Entry>();
}