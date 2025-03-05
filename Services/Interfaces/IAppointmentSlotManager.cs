using Model;

namespace Services.Interfaces;

/// <summary>
/// This interface manages appointment slots.
/// </summary>
public interface IAppointmentSlotManager
{
    List<AppointmentSlot> GetAvailableSlots(string specialty);
    bool BookSlot(AppointmentSlot slot);
}