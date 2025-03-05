namespace Model;

/// <summary>
/// This class represents an appointment.
/// </summary>
public class Appointment
{
    public required string Phone { get; set; }
    public DateTime FechaHora { get; set; }
    public required string Especialidad { get; set; }
    public bool NotificacionEnviada { get; set; } = false;
}