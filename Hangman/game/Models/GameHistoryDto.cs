namespace Hangman.Models
{
    public class GameHistoryDto
    {
        public string Word { get; set; }
        public bool IsWin { get; set; }
        public int AttemptsLeft { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}