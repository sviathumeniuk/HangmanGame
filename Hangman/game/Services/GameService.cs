using Hangman.Models;
using Hangman.Data;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Services
{
    public class HangmanService : IHangmanService
    {
        private readonly GameDbContext _dbContext;

        public HangmanService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

public async Task<GameResponse> StartGameAsync(int userId, Word filter, bool force)
{
    var existingGame = await _dbContext.GameStates
        .FirstOrDefaultAsync(g => g.UserId == userId && g.AttemptsLeft > 0 && !g.IsWin);

    if (existingGame != null && force)
    {
        existingGame.AttemptsLeft = 0;
        existingGame.EndTime = DateTime.UtcNow;
        _dbContext.GameStates.Update(existingGame);
        await _dbContext.SaveChangesAsync();
    }

    var randomWord = await _dbContext.Words
        .Where(w => w.Language == filter.Language
                    && w.DifficultyLevel == filter.DifficultyLevel
                    && w.Category == filter.Category)
        .OrderBy(w => EF.Functions.Random())
        .Select(w => w.Text)
        .FirstOrDefaultAsync();

    if (string.IsNullOrEmpty(randomWord))
    {
        return new GameResponse
        {
            Message = "No words available for the selected settings."
        };
    }

    var gameState = new GameState
    {
        UserId = userId,
        Word = randomWord.ToUpper(),
        AttemptsLeft = 6,
        GuessedLetters = string.Empty,
        StartTime = DateTime.UtcNow,
        IsWin = false
    };

    _dbContext.GameStates.Add(gameState);
    await _dbContext.SaveChangesAsync();

    return await ConnectToGameAsync(userId);
}

        public async Task<GameResponse> ConnectToGameAsync(int userId)
        {
            var activeGame = await _dbContext.GameStates
                .FirstOrDefaultAsync(g => g.UserId == userId && !g.IsWin && g.AttemptsLeft > 0);

            if (activeGame == null)
            {
                return new GameResponse
                {
                    Message = "No active game found."
                };
            }

            return new GameResponse
            {
                WordTemplate = GetWordTemplate(activeGame),
                AttemptsLeft = activeGame.AttemptsLeft,
                GameStatus = "In Progress",
                IsWin = activeGame.IsWin
            };
        }

        public async Task<GameResponse> MakeGuessAsync(int userId, GuessRequest request)
        {
            var gameState = await _dbContext.GameStates
                .FirstOrDefaultAsync(g => g.UserId == userId && g.AttemptsLeft > 0 && !g.IsWin);

            if (gameState == null)
            {
                return new GameResponse
                {
                    Message = "Game has not started yet or is over. Please start a new game."
                };
            }

            char guessedLetter = char.ToUpper(request.Letter);

            if (gameState.GuessedLetters.Contains(guessedLetter))
            {
                return new GameResponse
                {
                    Message = "Letter already guessed."
                };
            }

            AddGuessedLetter(gameState, guessedLetter);

            if (!gameState.Word.Contains(guessedLetter))
            {
                gameState.AttemptsLeft--;
            }

            gameState.IsWin = CheckWin(gameState);

            var word = await _dbContext.Words
                .FirstOrDefaultAsync(w => w.Text == gameState.Word);

            int difficulty = word?.DifficultyLevel ?? 0;

            if (gameState.AttemptsLeft <= 0 || gameState.IsWin)
            {
                gameState.EndTime = DateTime.UtcNow;
            }

            _dbContext.GameStates.Update(gameState);
            await _dbContext.SaveChangesAsync();

            return new GameResponse
            {
                WordTemplate = GetWordTemplate(gameState),
                AttemptsLeft = gameState.AttemptsLeft,
                GameStatus = (gameState.AttemptsLeft <= 0 || gameState.IsWin) ? "Game Over" : "In Progress",
                IsWin = gameState.IsWin,
                Message = gameState.IsWin ? "You win!" : (gameState.AttemptsLeft <= 0 || gameState.IsWin ? "Game Over!" : "Keep guessing!")
            };
        }

        public async Task<IEnumerable<GameHistoryDto>> GetGameHistoryAsync(int userId)
        {
            var gameHistory = await _dbContext.GameStates
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.StartTime)
                .Select(g => new GameHistoryDto
                {
                    Word = g.Word,
                    IsWin = g.IsWin,
                    AttemptsLeft = g.AttemptsLeft,
                    StartTime = g.StartTime,
                    EndTime = g.EndTime
                })
                .ToListAsync();

            return gameHistory;
        }

        private string GetWordTemplate(GameState gameState)
        {
            return string.Join(" ", gameState.Word.Select(c => gameState.GuessedLetters.Contains(c) ? c : '_'));
        }

        private bool CheckWin(GameState gameState)
        {
            return gameState.Word.All(c => gameState.GuessedLetters.Contains(c));
        }

        private void AddGuessedLetter(GameState gameState, char letter)
        {
            if (!gameState.GuessedLetters.Contains(letter))
            {
                gameState.GuessedLetters += letter;
            }
        }
    }
}