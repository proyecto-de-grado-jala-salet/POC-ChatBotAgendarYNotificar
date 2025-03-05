using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Services.Interfaces;

namespace Services;

/// <summary>
/// This class creates events in Google Calendar.
/// </summary>
public class GoogleCalendarService(IConfiguration configuration) : IGoogleCalendarService
{
    private readonly string? _credentials = configuration["GoogleCalendar:CredentialsPath"];
    private readonly string? _idCalendar = configuration["GoogleCalendar:IdCalendar"];

    private readonly string _timeZone = "America/La_Paz";

    /// <summary>
    /// Creates a new event in Google Calendar.
    /// </summary>
    /// <param name="startTime">The start time of the event.</param>
    /// <param name="specialty">The specialty of the appointment.</param>
    /// <returns>True if the event was created; false otherwise.</returns>
    public async Task<bool> CreateEventAsync(DateTime startTime, string specialty)
    {
        try
        {
            // Cargar las credenciales desde un archivo JSON
            GoogleCredential credential;
            using (var stream = new FileStream(_credentials!, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(CalendarService.Scope.Calendar);
            }

            // Inicializar el servicio de Calendar
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "ReservaCitas"
            });

            // Suponemos que la cita dura 45 min
            DateTime endTime = startTime.AddMinutes(45);

            Event newEvent = new Event()
            {
                Summary = "Cita " + specialty,
                Start = new EventDateTime()
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(startTime, TimeZoneInfo.FindSystemTimeZoneById(_timeZone).GetUtcOffset(startTime)),
                    TimeZone = _timeZone
                },
                End = new EventDateTime()
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(endTime, TimeZoneInfo.FindSystemTimeZoneById(_timeZone).GetUtcOffset(endTime)),
                    TimeZone = _timeZone
                }
            };

            // Usamos el calendario "primary"
            string calendarId = _idCalendar!;
            var request = service.Events.Insert(newEvent, calendarId);
            await request.ExecuteAsync();

            return true;
        }
        catch (Exception ex)
        {
            // Imprime o registra el error para depuración
            Console.WriteLine("Error al crear el evento: " + ex);
            return false;
        }
    }

}