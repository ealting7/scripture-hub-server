namespace scripture_hub_server.Infrastructure.Data.Models.Bible
{
    public class Chapters
    {
        public string TranslationId { get; set; } = string.Empty;
        public string? BookName { get; set; }
        public string? BookAbbreviation { get; set; }
        public int[] ChapterNumbers { get; set; } = [];
    }
}
