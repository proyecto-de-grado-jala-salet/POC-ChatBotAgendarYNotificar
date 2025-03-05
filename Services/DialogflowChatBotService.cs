using Google.Cloud.Dialogflow.V2;
using Model;
using Repositories;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class DialogflowChatBotService : IChatBotService
    {
        private readonly SessionsClient _sessionsClient;
        private readonly string _projectId;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IAppointmentSlotManager _slotManager;
        private readonly IGoogleCalendarService _calendarService;
        private readonly IConversationManager _conversationManager;
        private readonly InMemoryAppointmentRepository _appointmentRepository;

        public DialogflowChatBotService(
            IWhatsAppService whatsAppService,
            IAppointmentSlotManager slotManager,
            IGoogleCalendarService calendarService,
            IConversationManager conversationManager,
            InMemoryAppointmentRepository appointmentRepository,
            string projectId,
            string credentialsPath)
        {
            _whatsAppService = whatsAppService;
            _slotManager = slotManager;
            _calendarService = calendarService;
            _conversationManager = conversationManager;
            _appointmentRepository = appointmentRepository;
            _projectId = projectId;

            var builder = new SessionsClientBuilder { CredentialsPath = credentialsPath };
            _sessionsClient = builder.Build();
        }

        public async Task ProcessMessageAsync(string phone, string message)
        {
            var sessionPath = $"projects/{_projectId}/agent/sessions/{phone}";
            var queryInput = new QueryInput
            {
                Text = new TextInput { Text = message, LanguageCode = "es" }
            };

            var response = await _sessionsClient.DetectIntentAsync(sessionPath, queryInput);
            var intent = response.QueryResult.Intent.DisplayName;
            var parameters = response.QueryResult.Parameters;

            // Extraer la especialidad del contexto si no está en los parámetros
            string specialtyFromContext = ExtractSpecialtyFromContext(response.QueryResult.OutputContexts);
            if (!string.IsNullOrEmpty(specialtyFromContext))
            {
                _conversationManager.SetSpecialty(phone, specialtyFromContext);
            }

            switch (intent)
            {
                case "Greeting":
                    var fulfillmentText = response.QueryResult.FulfillmentText;
                    if (!string.IsNullOrEmpty(fulfillmentText))
                    {
                        await _whatsAppService.SendTextMessageAsync(phone, fulfillmentText);
                    }
                    else
                    {
                        await _whatsAppService.SendTextMessageAsync(phone, "¡Hola! ¿En qué puedo ayudarte hoy? ¿Quieres reservar una cita o consultar horarios?");
                    }
                    break;
                case "BookAppointment":
                    await HandleBookAppointment(phone, parameters);
                    break;
                case "CheckAvailability":
                    await HandleCheckAvailability(phone, parameters);
                    break;
                default:
                    await _whatsAppService.SendTextMessageAsync(phone, "No entendí. ¿Quieres reservar una cita o consultar horarios?");
                    break;
            }
        }

        private string ExtractSpecialtyFromContext(Google.Protobuf.Collections.RepeatedField<Context> contexts)
        {
            if (contexts == null || !contexts.Any())
                return string.Empty;

            var specialtyContext = contexts.FirstOrDefault(c => c.Name.Contains(Constants.EspecialidadKey));
            if (specialtyContext != null && specialtyContext.Parameters != null)
            {
                var specialtyParam = specialtyContext.Parameters.Fields.ContainsKey(Constants.EspecialidadKey) ?
                    specialtyContext.Parameters.Fields[Constants.EspecialidadKey].StringValue : null;
                return specialtyParam ?? string.Empty;
            }

            return string.Empty;
        }

        private async Task HandleBookAppointment(string phone, Google.Protobuf.WellKnownTypes.Struct parameters)
        {
            string specialty = GetOrSetSpecialty(phone, parameters);
            DateTime? appointmentDateTime = GetOrSetAppointmentDateTime(phone, parameters);

            if (string.IsNullOrEmpty(specialty))
            {
                await _whatsAppService.SendTextMessageAsync(phone, "Por favor, indica la especialidad (Psicología o Fisioterapia).");
                return;
            }
            if (appointmentDateTime == null)
            {
                await _whatsAppService.SendTextMessageAsync(phone, "Por favor, indica la fecha y hora (ejemplo: 10 de febrero a las 21:00 o 10/02/2025 21:00).");
                return;
            }

            var slot = new AppointmentSlot { StartTime = appointmentDateTime.Value, Specialty = specialty };
            if (_slotManager.BookSlot(slot))
            {
                bool eventCreated = await _calendarService.CreateEventAsync(appointmentDateTime.Value, specialty);
                if (eventCreated)
                {
                    var appointment = new Appointment
                    {
                        Phone = phone,
                        FechaHora = appointmentDateTime.Value,
                        Especialidad = specialty,
                        NotificacionEnviada = false
                    };
                    _appointmentRepository.SaveAppointment(appointment);

                    await _whatsAppService.SendTextMessageAsync(phone, $"Cita de {specialty} reservada para {appointmentDateTime.Value:dd/MM/yyyy HH:mm}.");
                    _conversationManager.ClearState(phone);
                }
                else
                {
                    await _whatsAppService.SendTextMessageAsync(phone, "Hubo un error al crear el evento en Google Calendar.");
                }
            }
            else
            {
                await _whatsAppService.SendTextMessageAsync(phone, "Lo siento, ese horario no está disponible. Consulta los horarios con 'horarios disponibles'.");
            }
        }


        private string GetOrSetSpecialty(string phone, Google.Protobuf.WellKnownTypes.Struct parameters)
{
    string specialty;
    if (parameters.Fields.ContainsKey(Constants.EspecialidadKey) &&
        !string.IsNullOrWhiteSpace(parameters.Fields[Constants.EspecialidadKey].StringValue))
    {
        specialty = parameters.Fields[Constants.EspecialidadKey].StringValue;
        _conversationManager.SetSpecialty(phone, specialty);
        Console.WriteLine($"Especialidad establecida desde parámetros: {specialty}");
    }
    else
    {
        specialty = _conversationManager.GetSpecialty(phone);
        Console.WriteLine($"Especialidad recuperada desde ConversationManager: {specialty}");
    }
    return specialty;
}

        private DateTime? GetOrSetAppointmentDateTime(string phone, Google.Protobuf.WellKnownTypes.Struct parameters)
        {
            DateTime? appointmentDateTime = null;
            if (parameters.Fields.ContainsKey("date-time"))
            {
                var dateTimeField = parameters.Fields["date-time"];
                if (!string.IsNullOrEmpty(dateTimeField.StringValue))
                {
                    try
                    {
                        appointmentDateTime = DateTime.Parse(dateTimeField.StringValue, new System.Globalization.CultureInfo("es-ES"), System.Globalization.DateTimeStyles.RoundtripKind);
                    }
                    catch (FormatException)
                    {
                        try
                        {
                            appointmentDateTime = DateTime.Parse(dateTimeField.StringValue, new System.Globalization.CultureInfo("es-ES"));
                        }
                        catch (FormatException)
                        {
                            appointmentDateTime = null;
                        }
                    }
                }
                else if (dateTimeField.StructValue != null && dateTimeField.StructValue.Fields.ContainsKey("date_time"))
                {
                    var dtValue = dateTimeField.StructValue.Fields["date_time"].StringValue;
                    try
                    {
                        appointmentDateTime = DateTime.Parse(dtValue, new System.Globalization.CultureInfo("es-ES"), System.Globalization.DateTimeStyles.RoundtripKind);
                    }
                    catch (FormatException)
                    {
                        try
                        {
                            appointmentDateTime = DateTime.Parse(dtValue, new System.Globalization.CultureInfo("es-ES"));
                        }
                        catch (FormatException)
                        {
                            appointmentDateTime = null;
                        }
                    }
                }
                var state = _conversationManager.GetState(phone);
                state.AppointmentDateTime = appointmentDateTime;
            }
            else
            {
                appointmentDateTime = _conversationManager.GetState(phone).AppointmentDateTime;
            }
            return appointmentDateTime;
        }


        private async Task HandleCheckAvailability(string phone, Google.Protobuf.WellKnownTypes.Struct parameters)
        {
            string specialty = GetOrSetSpecialty(phone, parameters);
            if (string.IsNullOrEmpty(specialty))
            {
                await _whatsAppService.SendTextMessageAsync(phone, "Por favor, indica la especialidad (Psicología o Fisioterapia).");
                return;
            }

            var slots = _slotManager.GetAvailableSlots(specialty);
            if (slots.Any())
            {
                string response = $"Horarios disponibles para {specialty}:\n" +
                                  string.Join("\n", slots.Select(s => s.StartTime.ToString("dd/MM/yyyy HH:mm")));
                await _whatsAppService.SendTextMessageAsync(phone, response);
            }
            else
            {
                await _whatsAppService.SendTextMessageAsync(phone, $"No hay horarios disponibles para {specialty}.");
            }
        }

    }
}