using Model;
using System.Collections.Concurrent;

namespace Repositories;

/// <summary>
/// This repository stores appointments in memory using a thread-safe dictionary.
/// </summary>
public class InMemoryAppointmentRepository
{
    private readonly ConcurrentDictionary<string, Appointment> _appointments = new();

    /// <summary>
    /// Saves an appointment in memory.
    /// </summary> 
    /// <param name="appointment">The appointment to save.</param>
    public void SaveAppointment(Appointment appointment)
    {
        _appointments[appointment.Phone] = appointment;
    }

    /// <summary>
    /// Gets all appointments within a given time range.
    /// </summary>
    /// <param name="start">The start time of the range.</param>
    /// <param name="end">The end time of the range.</param>
    /// <returns>A list of appointments in that range.</returns>
    public List<Appointment> GetAppointmentsInRange(DateTime start, DateTime end)
    {
        return _appointments.Values
            .Where(a => a.FechaHora >= start && a.FechaHora <= end)
            .ToList();
    }

    /// <summary>
    /// Updates an existing appointment in memory.
    /// </summary>
    /// <param name="appointment">The appointment to update.</param>
    public void UpdateAppointment(Appointment appointment)
    {
        _appointments[appointment.Phone] = appointment;
    }
}
