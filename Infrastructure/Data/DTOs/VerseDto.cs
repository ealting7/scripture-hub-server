namespace scripture_hub_server.Infrastructure.Data.DTOs
{
    public class VerseDto
    {
        public int Id { get; set; }
        public string TranslationId { get; set; } = string.Empty;
        public int BookId { get; set; }
        public int Chapter { get; set; }
        public int VerseNumber { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? HighlightColor { get; set; }
        public bool IsDailyScripture { get; set; }
    }
}
