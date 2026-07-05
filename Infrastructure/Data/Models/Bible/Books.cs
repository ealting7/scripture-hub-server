namespace scripture_hub_server.Infrastructure.Data.Models.Bible
{
    public class Books
    {
        public required int Id { get; set;  } = 0;
        public required string TranslationId { get; set; } = string.Empty;
        public required string Name { get; set; } = string.Empty;
        public required string Testament { get; set; } = string.Empty;

        public required string Abbreviation { get; set; } = string.Empty;

        public required int BookId { get; set; } = 0;
    }
}
