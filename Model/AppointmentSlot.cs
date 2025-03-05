namespace Model;

/// <summary> 
/// This class represents a time slot for an appointment.
/// </summary>
public class AppointmentSlot
{
    public DateTime StartTime { get; set; }
    public required string Specialty { get; set; }
    public bool Booked { get; set; }
}