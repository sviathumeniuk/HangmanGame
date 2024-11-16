using Auth.Models;
using Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
        }

        [HttpGet("profile/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var currentUserId = GetUserId();
            if (currentUserId != userId)
            {
                return Forbid("You are not authorized to access this profile.");
            }

            var result = await _authService.GetUserProfileAsync(userId);
            if (!(bool)((dynamic)result).Success)
            {
                return NotFound(new { Message = ((dynamic)result).Message });
            }

            return Ok(((dynamic)result).User);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(model);
            if (!(bool)((dynamic)result).Success)
            {
                return BadRequest(new { Message = ((dynamic)result).Message });
            }

            return Ok(new { Message = ((dynamic)result).Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(model);
            if (!(bool)((dynamic)result).Success)
            {
                return Unauthorized(new { Message = ((dynamic)result).Message });
            }

            return Ok(new { Token = ((dynamic)result).Token });
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID claim is missing in the token.");
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new FormatException("User ID claim is not a valid integer.");
            }

            return userId;
        }
    }
}