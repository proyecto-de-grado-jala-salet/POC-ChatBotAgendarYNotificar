using Background;
using Repositories;
using Services;
using Services.Interfaces;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Deshabilitar el monitoreo automático de cambios en todos los archivos de configuración
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
                     .AddEnvironmentVariables();

// Si usas variables de entorno o otros orígenes, asegúrate de deshabilitar reloadOnChange
// builder.Configuration.AddEnvironmentVariables(reloadOnChange: false); // Si es necesario

// Registro de servicios
builder.Services.AddHttpClient<WhatsAppService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();

builder.Services.AddSingleton<IConversationManager, ConversationManager>();
builder.Services.AddSingleton<InMemoryAppointmentRepository>();
builder.Services.AddSingleton<IAppointmentSlotManager, AppointmentSlotManager>();

builder.Services.AddHostedService<AppointmentReminderService>();

var projectId = builder.Configuration["Dialogflow:ProjectId"] ?? throw new InvalidOperationException("ProjectId no está configurado.");
var credentialsPath = builder.Configuration["Dialogflow:CredentialsPath"] ?? throw new InvalidOperationException("CredentialsPath no está configurado.");

builder.Services.AddScoped<DialogflowChatBotService>(sp =>
{
    return new DialogflowChatBotService(
        sp.GetRequiredService<IWhatsAppService>(),
        sp.GetRequiredService<IAppointmentSlotManager>(),
        sp.GetRequiredService<IGoogleCalendarService>(),
        sp.GetRequiredService<IConversationManager>(),
        sp.GetRequiredService<InMemoryAppointmentRepository>(),
        projectId,
        credentialsPath);
});
builder.Services.AddScoped<IChatBotService>(sp => sp.GetRequiredService<DialogflowChatBotService>());
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();