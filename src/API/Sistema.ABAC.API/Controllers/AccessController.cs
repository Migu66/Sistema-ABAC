using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.Services.ABAC;
using System.ComponentModel.DataAnnotations;

namespace Sistema.ABAC.API.Controllers;

/// <summary>
/// Controlador para evaluación de acceso ABAC.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccessController : ControllerBase
{
    private readonly IAccessControlService _accessControlService;
    private readonly ILogger<AccessController> _logger;

    /// <summary>
    /// Constructor del controlador de acceso.
    /// </summary>
    public AccessController(
        IAccessControlService accessControlService,
        ILogger<AccessController> logger)
    {
        _accessControlService = accessControlService;
        _logger = logger;
    }

    /// <summary>
    /// Evalúa una solicitud de acceso ABAC.
    /// </summary>
    /// <param name="request">Datos de evaluación: usuario, recurso, acción y contexto opcional</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado básico del procesamiento de evaluación</returns>
    [HttpPost("evaluate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<object>> Evaluate(
        [FromBody] EvaluateAccessRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Iniciando evaluación ABAC para UserId={UserId}, ResourceId={ResourceId}, ActionId={ActionId}",
            request.UserId,
            request.ResourceId,
            request.ActionId);

        var result = await _accessControlService.CheckAccessAsync(
            request.UserId,
            request.ResourceId,
            request.ActionId,
            request.Context,
            cancellationToken);

        return Ok(new
        {
            Processed = true,
            Decision = result.Decision.ToString()
        });
    }
}

/// <summary>
/// DTO para solicitudes de evaluación de acceso ABAC.
/// </summary>
public class EvaluateAccessRequest
{
    /// <summary>
    /// ID del usuario a evaluar.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// ID del recurso al que se quiere acceder.
    /// </summary>
    [Required]
    public Guid ResourceId { get; set; }

    /// <summary>
    /// ID de la acción solicitada.
    /// </summary>
    [Required]
    public Guid ActionId { get; set; }

    /// <summary>
    /// Atributos opcionales de contexto para la evaluación.
    /// </summary>
    public Dictionary<string, object?>? Context { get; set; }
}