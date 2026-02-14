using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.Services.ABAC;

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
}