using scripture_hub_server.Infrastructure.Data.Models.Bible;
using scripture_hub_server.Infrastructure.Data.DTOs;

namespace scripture_hub_server.Application.Interfaces
{
    public interface IBibleService
    {
        Task<List<Translations>?> GetBibleTranslation();
        Task<List<Books>?> GetBooksForTranslation(string translationId);
        Task<List<BookInfo>?> GetTranslationBookInfo(string translationId);
        Task<List<Chapters>?> GetAllChaptersForAllBooks(string translationId);
        Task<Chapters?> GetChaptersForTranslation(string translationId, int bookId);
        Task<List<VerseDto>?> GetScriptureForTranslation(string translationId, int bookId, int chapter);



        Task FixSqlFile();
    }
}
