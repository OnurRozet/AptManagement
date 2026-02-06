using AptManagement.Application.Dtos;
using AptManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        var result = await authService.RegisterAsync(request);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var result = await authService.LoginAsync(request);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        // Şimdilik sadece mesaj dönüyoruz, ilerde buraya Token eklenecek.    
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync();
        return Ok(new { Message = "Logout successful" });
    }

    [HttpGet("get-user-info/{apartmentNumber}")]
    public async Task<IActionResult> GetUserInfo(string apartmentNumber)
    {
        var result = await authService.GetUserInfoAsync(apartmentNumber);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Ok(result);
    }
}
