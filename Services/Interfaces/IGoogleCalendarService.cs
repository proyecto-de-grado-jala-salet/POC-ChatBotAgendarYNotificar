namespace Services.Interfaces;

/// <summary>
/// This interface defines a service to create events in Google Calendar.
/// </summary>
public interface IGoogleCalendarService
{
    Task<bool> CreateEventAsync(DateTime startTime, string specialty);
}
