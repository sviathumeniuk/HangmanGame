using Hangman.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hangman.Services
{
    public interface IHangmanService
    {
        Task<GameResponse> StartGameAsync(int userId, Word filter, bool force);
        Task<GameResponse> ConnectToGameAsync(int userId);
        Task<GameResponse> MakeGuessAsync(int userId, GuessRequest request);
        Task<IEnumerable<GameHistoryDto>> GetGameHistoryAsync(int userId);
    }
}