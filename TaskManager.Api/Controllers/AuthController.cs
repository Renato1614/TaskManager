using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Services;
using TaskManager.Application.Auth;
using TaskManager.Application.Common;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        return Created("/api/auth/me", response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.GetCurrentUserAsync(currentUser.UserId, cancellationToken);
            return Ok(response);
        }
        catch (AppException ex) when (ex.Type == AppErrorType.NotFound)
        {
            return NoContent();
        }
    }

    [HttpGet("public-demo")]
    [AllowAnonymous]
    public ActionResult<object> PublicDemo() => Ok(new { message = "Anyone can see this endpoint." });

    [HttpGet("private-demo")]
    [Authorize]
    public ActionResult<object> PrivateDemo() => Ok(new { message = "Only authenticated users can see this endpoint." });
}
