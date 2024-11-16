namespace Hangman.Models
{
    public class GameState
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Word { get; set; }
        public string GuessedLetters { get; set; } = string.Empty;
        public int AttemptsLeft { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsWin { get; set; }

    }
}