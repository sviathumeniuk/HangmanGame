using Hangman.Models;
using Hangman.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class HangmanController : ControllerBase
    {
        private readonly IHangmanService _hangmanService;

        public HangmanController(IHangmanService hangmanService)
        {
            _hangmanService = hangmanService;
        }

        [HttpPost("start")]
        [Authorize]
        public async Task<IActionResult> StartGame([FromBody] Word filter, [FromQuery] bool force = false)
        {
            try
            {
                int userId = GetUserId();
                var response = await _hangmanService.StartGameAsync(userId, filter, force);
                if (!string.IsNullOrEmpty(response.Message))
                {
                    return BadRequest(new { Message = response.Message });
                }
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (FormatException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("connect")]
        [Authorize]
        public async Task<IActionResult> ConnectToGame()
        {
            try
            {
                int userId = GetUserId();
                var response = await _hangmanService.ConnectToGameAsync(userId);
                if (!string.IsNullOrEmpty(response.Message))
                {
                    return NotFound(new { Message = response.Message });
                }
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }

        [HttpPost("guess")]
        [Authorize]
        public async Task<IActionResult> MakeGuess([FromBody] GuessRequest request)
        {
            try
            {
                int userId = GetUserId();
                var response = await _hangmanService.MakeGuessAsync(userId, request);
                if (!string.IsNullOrEmpty(response.Message) && response.Message.Contains("Game"))
                {
                    return BadRequest(new { Message = response.Message });
                }
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }

        [HttpGet("history/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetGameHistory(int userId)
        {
            var gameHistory = await _hangmanService.GetGameHistoryAsync(userId);
            return Ok(gameHistory);
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("nameid")?.Value;
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