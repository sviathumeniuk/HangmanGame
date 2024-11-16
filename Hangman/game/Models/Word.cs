namespace Hangman.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string Category { get; set; }
        public string Language { get; set; }
        public int DifficultyLevel { get; set; }
    }
}