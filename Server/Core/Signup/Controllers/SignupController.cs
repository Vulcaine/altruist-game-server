using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Server.Signup;

[ApiController]
[Route("api/[controller]")]
public sealed class SignupController : ControllerBase
{
    private readonly SignupService _signupService;

    public SignupController(SignupService signupService)
    {
        _signupService = signupService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest req)
    {
        var result = await _signupService.Signup(req.Email, req.Username, req.Password);

        if (!result.Success)
        {
            return Conflict(new { message = result.Error });
        }

        var resp = new SignupResponse
        {
            UserId = result.UserId!,
            Username = result.Username!,
            Email = result.Email!,
            RequiresEmailVerification = result.RequiresEmailVerification,
            Verification = result.Verification,
            NextStep = result.RequiresEmailVerification ? "verify_email" : "login"
        };

        return CreatedAtAction(nameof(GetUser), new { id = resp.UserId }, resp);
    }

    [HttpGet("verify")]
    public async Task<IActionResult> Verify([FromQuery] string uid, [FromQuery] string token)
    {
        var res = await _signupService.VerifyEmail(uid, token);
        if (!res.Success) return BadRequest(new { message = res.Error });
        return Ok(new { message = "Email verified" });
    }

    [HttpGet("users/{id}")]
    public IActionResult GetUser(string id) => Ok(new { id });
}
