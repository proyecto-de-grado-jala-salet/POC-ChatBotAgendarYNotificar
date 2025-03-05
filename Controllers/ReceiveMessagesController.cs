using Microsoft.AspNetCore.Mvc;
using Model;
using Services.Interfaces;

namespace Controller;

/// <summary> 
/// This controller handles incoming webhook messages. 
/// </summary>
[ApiController]
[Route("webhook")]
public class ReceiveMessagesController : ControllerBase
{
    private readonly IChatBotService _chatBotService;

    /// <summary>
    /// This constructor sets up the controller with a chat bot service.
    /// </summary>
    /// <param name="chatBotService">The service to process messages.</param>
    public ReceiveMessagesController(IChatBotService chatBotService)
    {
        _chatBotService = chatBotService;
    }

    /// <summary>
    /// This GET endpoint is used to verify the webhook.
    /// </summary>
    /// <param name="mode">The mode from the query.</param>
    /// <param name="challenge">A challenge string from the query.</param>
    /// <param name="verifyToken">The token to verify the webhook.</param>
    /// <returns>The challenge if the token is correct; otherwise, Unauthorized.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.challenge")] string challenge,
        [FromQuery(Name = "hub.verify_token")] string verifyToken)
    {
        if (verifyToken == "hola")
        {
            return Ok(challenge);
        }
        return Unauthorized();
    }

    /// <summary>
    /// This POST endpoint processes incoming webhook messages.
    /// It gets the message and sends it to the chat bot service.
    /// </summary>
    /// <param name="webhookResponse">The webhook response model.</param>
    /// <returns>An action result indicating success or error.</returns>
    [HttpPost]
    public async Task<IActionResult> PostWebhook([FromBody] WebHookResponseModel webhookResponse)
    {
        if (webhookResponse?.Entry == null || webhookResponse.Entry.Length == 0)
        {
            return BadRequest();
        }

        var entry = webhookResponse.Entry.FirstOrDefault();
        var change = entry?.Changes?.FirstOrDefault();
        
        var message = change?.value?.Messages.FirstOrDefault();

        if (message == null)
        {
            return BadRequest();
        }

        string incomingMessage = message.Text.body;
        string fromPhone = message.From;

        if (!string.IsNullOrEmpty(incomingMessage))
        {
            await _chatBotService.ProcessMessageAsync(fromPhone, incomingMessage);
        }

        return Ok();
    }
}