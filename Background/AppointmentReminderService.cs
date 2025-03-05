using Repositories;
using Services.Interfaces;
using System.Globalization;

namespace Background;

/// <summary>
/// This class runs in the background and sends appointment reminders.
/// It checks every 30 seconds for appointments that are about to happen.
/// </summary>
public class AppointmentReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    /// <summary>
    /// This is the time interval between checks (30 seconds).
    /// </summary>
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// This constructor sets up the service with a scope factory.
    /// </summary>
    /// <param name="scopeFactory">A factory to create service scopes.</param>
    public AppointmentReminderService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// This method runs the background task. It looks for soon-to-start
    /// appointments and sends a WhatsApp reminder message.
    /// </summary>
    /// <param name="stoppingToken">A token to cancel the task.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var appointmentRepository = scope.ServiceProvider.GetRequiredService<InMemoryAppointmentRepository>();
                var whatsAppService = scope.ServiceProvider.GetRequiredService<IWhatsAppService>();

                DateTime ahora = DateTime.Now;
                DateTime recordatorioInicio = ahora.AddMinutes(1);
                DateTime recordatorioFin = ahora.AddMinutes(2);

                var citasPendientes = appointmentRepository.GetAppointmentsInRange(recordatorioInicio, recordatorioFin)
                    .Where(c => !c.NotificacionEnviada)
                    .ToList();

                Console.WriteLine($"[ReminderService] Encontradas {citasPendientes.Count} citas pendientes.");

                foreach (var cita in citasPendientes)
                {
                    string mensaje = $"Recordatorio: Tiene una cita programada a las {cita.FechaHora.ToString("H:mm, dd MMM yyyy", new CultureInfo("es-ES"))} para {cita.Especialidad}.";
                    await whatsAppService.SendTextMessageAsync(cita.Phone, mensaje);

                    cita.NotificacionEnviada = true;
                    appointmentRepository.UpdateAppointment(cita);
                }
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}