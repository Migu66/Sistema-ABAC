using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.Services.ABAC;
using AbacAuthorizationResult = Sistema.ABAC.Application.Services.ABAC.AuthorizationResult;

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
    /// <returns>Resultado completo de autorización ABAC</returns>
    /// <response code="200">Evaluación de acceso realizada correctamente</response>
    /// <response code="400">Solicitud inválida</response>
    /// <response code="401">Usuario no autenticado</response>
    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(AbacAuthorizationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AbacAuthorizationResult>> Evaluate(
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

        return Ok(result);
    }
}