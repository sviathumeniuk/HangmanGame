namespace Auth.Models
{
    public class RatingUpdateMessage
    {
        public int UserId { get; set; }
        public bool IsWin { get; set; }
        public int Difficulty { get; set; }
    }
}