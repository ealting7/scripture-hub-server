using Microsoft.EntityFrameworkCore;
using scripture_hub_server.Infrastructure.Data.Models.Auth;
using scripture_hub_server.Infrastructure.Data.Models.Bible;

namespace scripture_hub_server.Infrastructure.Data.Context
{
    public class ScriptureHubDbContext : DbContext
    {

        public ScriptureHubDbContext(DbContextOptions<ScriptureHubDbContext> options) : base(options) {}

        //public DbSet<AppUser> Users => Set<AppUser>();        
        public DbSet<Translations> BibleTranslations { get; set; }
        public DbSet<Books> Books { get; set; }
        public DbSet<Verses> Verses { get; set; }

        public DbSet<UserVerses> UserVerses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("ScriptureHub");

            modelBuilder.Entity<Translations>(entity =>
            {
                entity.ToTable("Translations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").IsRequired();
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.License).HasColumnName("license");
                entity.Property(e => e.UsesApi).HasColumnName("uses_api");
                entity.Property(e => e.ApiUrl).HasColumnName("api_url");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.TypeId).HasColumnName("translation_type_id");
                entity.Property(e => e.PaymentTier).HasColumnName("payment_tier");
            });

            modelBuilder.Entity<Books>(entity =>
            {
                entity.ToTable("Books");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").IsRequired();
                entity.Property(e => e.TranslationId).HasColumnName("translation_id").IsRequired();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Testament).HasColumnName("testament").IsRequired();
                entity.Property(e => e.Abbreviation).HasColumnName("abbreviation").IsRequired();
                entity.Property(e => e.BookId).HasColumnName("book_id").IsRequired();
            });

            modelBuilder.Entity<Verses>(entity =>
            {
                entity.ToTable("Verses", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").IsRequired();
                entity.Property(e => e.TranslationId).HasColumnName("translation_id").IsRequired();
                entity.Property(e => e.BookId).HasColumnName("book_id").IsRequired();
                entity.Property(e => e.Chapter).HasColumnName("chapter").IsRequired();
                entity.Property(e => e.VerseNumber).HasColumnName("verse").IsRequired();
                entity.Property(e => e.Text).HasColumnName("text");
            });


            modelBuilder.Entity<UserVerses>(entity =>
            {
                entity.ToTable("UserVerses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").IsRequired();
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.BookId).HasColumnName("book_id").IsRequired();
                entity.Property(e => e.Chapter).HasColumnName("chapter").IsRequired();
                entity.Property(e => e.VerseNumber).HasColumnName("verse").IsRequired();
                entity.Property(e => e.HighlightColor).HasColumnName("highlight_color");
                entity.Property(e => e.IsDailyScripture).HasColumnName("daily_scripture");
                entity.Property(e => e.DailyScriptureShownDate).HasColumnName("daily_scripture_date");
            });
        }
    }
}
