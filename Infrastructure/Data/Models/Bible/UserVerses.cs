namespace scripture_hub_server.Infrastructure.Data.Models.Bible
{
    public class UserVerses
    {
        public required int Id { get; set; } = 0;
        public required string UserId { get; set; } = string.Empty;
        public required int BookId { get; set; } = 0;
        public required int Chapter { get; set; } = 0;
        public required int VerseNumber { get; set; } = 0;
        public string? HighlightColor { get; set; }
        public bool IsDailyScripture { get; set; }
        public DateTimeOffset DailyScriptureShownDate { get; set; }
    }
}
