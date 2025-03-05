using Model;
using Services.Interfaces;

namespace Services;

/// <summary>
/// This class manages the appointment slots. 
/// It holds a list of time slots for different specialties.
/// </summary>
public class AppointmentSlotManager : IAppointmentSlotManager
{
    public const string PSYCHOLOGY_SPECIALTY = "Psicologia";
    public const string PHYSIOTHERAPY_SPECIALTY = "Fisioterapia";

    // Lista de horarios para cada especialidad (cada uno es independiente).
    private readonly List<AppointmentSlot> _slots = new List<AppointmentSlot>
    {
        // Horarios para Psicóloga
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 35, 0, DateTimeKind.Local), Specialty = PSYCHOLOGY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 45, 0, DateTimeKind.Local), Specialty = PSYCHOLOGY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 30, 0, DateTimeKind.Local), Specialty = PSYCHOLOGY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 0, 0, DateTimeKind.Local), Specialty = PSYCHOLOGY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 17, 0, DateTimeKind.Local), Specialty = PSYCHOLOGY_SPECIALTY, Booked = false },

        // Horarios para Fisioterapia (pueden ser iguales o diferentes; son independientes)
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 35, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 32, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 33, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 34, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 36, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 40, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 43, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 47, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 23, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
        new AppointmentSlot { StartTime = new DateTime(2025, 3, 4, 23, 25, 0, DateTimeKind.Local), Specialty = PHYSIOTHERAPY_SPECIALTY, Booked = false },
    };

    /// <summary>
    /// Gets the available slots for a given specialty.
    /// </summary>
    /// <param name="specialty">The chosen specialty.</param>
    /// <returns>A list of slots that are not booked.</returns>
    /// 

    public List<AppointmentSlot> GetAvailableSlots(string specialty)
    {
        Console.WriteLine($"Buscando slots para especialidad: {specialty}"); // Depuración
    return _slots
        .Where(s => s.Specialty.Equals(specialty, StringComparison.OrdinalIgnoreCase) && !s.Booked)
        .ToList();
    }


    /// <summary>
    /// Books an appointment slot if it is available.
    /// </summary>
    /// <param name="slot">The slot to book.</param>
    /// <returns>True if the slot was booked; false otherwise.</returns>
    public bool BookSlot(AppointmentSlot slot)
    {
        Console.WriteLine($"Intentando reservar slot para especialidad: {slot.Specialty}, hora: {slot.StartTime}"); // Depuración
    var found = _slots.FirstOrDefault(s =>
        s.Specialty.Equals(slot.Specialty, StringComparison.OrdinalIgnoreCase) &&
        s.StartTime == slot.StartTime &&
        !s.Booked);

    if (found != null)
    {
        found.Booked = true;
        return true;
    }
    return false;
    }
}