namespace Hangman.Models
{
    public class GameResponse
    {
        public string WordTemplate { get; set; }
        public int AttemptsLeft { get; set; }
        public string GameStatus { get; set; }
        public bool IsWin { get; set; }
        public string Message { get; internal set; }
    }
}