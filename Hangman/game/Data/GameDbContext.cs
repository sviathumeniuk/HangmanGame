using Microsoft.EntityFrameworkCore;
using Hangman.Models;

namespace Hangman.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        public DbSet<Word> Words { get; set; }
        public DbSet<GameState> GameStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Word>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Text)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(w => w.Category)
                      .HasMaxLength(50);
                entity.Property(w => w.Language)
                      .IsRequired()
                      .HasMaxLength(10);
                entity.Property(w => w.DifficultyLevel)
                      .IsRequired();
            });

            modelBuilder.Entity<GameState>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.UserId).IsRequired();
                entity.Property(g => g.Word).IsRequired().HasMaxLength(100);
                entity.Property(g => g.GuessedLetters).IsRequired();
                entity.Property(g => g.AttemptsLeft).IsRequired();
                entity.Property(g => g.StartTime).IsRequired();
                entity.Property(g => g.IsWin).IsRequired();
                entity.Property(g => g.EndTime);
            });
        }
    }
}