namespace scripture_hub_server.Infrastructure.Data.Models.Bible
{
    public class Verses
    {
        public required int Id { get; set; } = 0;
        public required string TranslationId { get; set; } = string.Empty;
        public required int BookId { get; set; } = 0;
        public required int Chapter { get; set; } = 0;
        public required int VerseNumber { get; set; } = 0;
        public string Text { get; set; } = string.Empty;
    }
}

