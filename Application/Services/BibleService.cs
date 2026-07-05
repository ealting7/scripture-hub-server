using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using scripture_hub_server.Application.Constants;
using scripture_hub_server.Application.Interfaces;
using scripture_hub_server.Infrastructure.Data.Context;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using StackExchange.Redis;
using scripture_hub_server.Infrastructure.Data.Models.Auth;
using scripture_hub_server.Infrastructure.Data.Models.Bible;
using System.Collections.Generic;
using scripture_hub_server.Infrastructure.Data.DTOs;

namespace scripture_hub_server.Application.Services
{
    public class BibleService : IBibleService
    {

        private readonly ScriptureHubDbContext _dbContext;
        private readonly ICacheService _cacheService;
        private readonly IUserContextAccessorService _userContextAccessorService;
        private readonly ILogger<BibleService> _logger;

        public BibleService(ScriptureHubDbContext dbContext, ICacheService cacheService,
            IUserContextAccessorService userContextAccessorService, ILogger<BibleService> logger)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
            _userContextAccessorService = userContextAccessorService;
            _logger = logger;
        }
        
        public async Task<List<Translations>?> GetBibleTranslation()
        {
            var user = _userContextAccessorService.UserContext;
            if (user == null)
            {
                return null;
            }

            return await GetDatabaseTranslations(user.UserId);
        }

        public async Task<List<BookInfo>?> GetTranslationBookInfo(string translationId)
        {
            try
            {
                var cacheKey = CacheKeys.BookInfo(translationId);
                var cachedBookInfo = await GetCachedData<List<BookInfo>?>(cacheKey);

                if (cachedBookInfo != null)
                    return cachedBookInfo;

                return await GetDatabaseTranslationBibleBooks(translationId, cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve translation book information:");
                return null;
            }
        }

        public async Task<List<Books>?> GetBooksForTranslation(string translationId)
        {
            try
            {
                var cacheKey = CacheKeys.Books(translationId);
                var cachedBooks = await GetCachedData<List<Books>?>(cacheKey);

                if (cachedBooks != null)
                    return cachedBooks;

                return await GetDatabaseBooks(translationId, cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve books for translation:"); ;
                return null;
            }
        }

        public async Task<List<Chapters>?> GetAllChaptersForAllBooks(string translationId)
        {
            try
            {
                List<Chapters> allChapters = new List<Chapters>();

                var cacheKey = CacheKeys.AllChapters(translationId);
                var cachedAllChapters = await GetCachedData<List<Chapters>?>(cacheKey);

                if (cachedAllChapters != null)
                    return cachedAllChapters;

                return await GetDatabaseAllChapters(translationId, cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all book chapters:");
                return null;
            }
        }

        public async Task<Chapters?> GetChaptersForTranslation(string translationId, int bookId)
        {
            try
            {
                var cacheKey = CacheKeys.Chapters(translationId, bookId);
                var cachedChapters = await GetCachedData<Chapters?>(cacheKey);

                if (cachedChapters != null)
                    return cachedChapters;

                return await GetDatabaseChapters(translationId, bookId, cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve chapters for translation:"); ;
                return null;
            }
        }

        public async Task<List<VerseDto>?> GetScriptureForTranslation(string translationId, int bookId, int chapter)
        {
            try
            {
                string cacheKey = CacheKeys.Scripture(translationId, bookId, chapter);
                var cachedScripture = await GetCachedData<List<Verses>?>(cacheKey);

                // fetch the canonical verses from cache/db first (unchanged)
                List<Verses>? verses = null;

                if (cachedScripture != null)
                {
                    verses = cachedScripture;
                }
                else
                {
                    verses = await GetDatabaseScripture(translationId, bookId, chapter, cacheKey);
                }

                if (verses == null)
                    return null;

                // build dto list from verses
                var result = verses.Select(v => new VerseDto
                {
                    Id = v.Id,
                    TranslationId = v.TranslationId,
                    BookId = v.BookId,
                    Chapter = v.Chapter,
                    VerseNumber = v.VerseNumber,
                    Text = v.Text,
                    HighlightColor = null,
                    IsDailyScripture = false
                }).ToList();

                // lookup user-specific UserVerses and apply highlights/daily flag
                var user = _userContextAccessorService.UserContext ?? GetTemporaryUser();

                var userVerses = await _dbContext.UserVerses
                    .Where(uv => uv.UserId == user.UserId && uv.BookId == bookId && uv.Chapter == chapter)
                    .ToListAsync();

                if (userVerses != null && userVerses.Count > 0)
                {
                    var uvLookup = userVerses.ToDictionary(u => (u.BookId, u.Chapter, u.VerseNumber));

                    foreach (var dto in result)
                    {
                        if (uvLookup.TryGetValue((dto.BookId, dto.Chapter, dto.VerseNumber), out var uv))
                        {
                            dto.HighlightColor = uv.HighlightColor;
                            dto.IsDailyScripture = uv.IsDailyScripture;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve scripture for translation:");
                return null;
            }
        }

        public Task<List<Books>> GetApiBooks(string translationId)
        {
            List<Books> books = new List<Books>();

            return Task.FromResult(books);
        }

        public Task<List<BookInfo>> GetApiTranslationBibleBooks(string translationId)
        {
            List<BookInfo> books = new List<BookInfo>();

            return Task.FromResult(books);
        }

        public Task<List<Chapters>> GetApiAllChapters(string translationId)
        {
            List<Chapters> chapters = new List<Chapters>();

            return Task.FromResult(chapters);
        }

        public Task<Chapters> GetApiChapters(string translationId, int bookId)
        {
            Chapters chapters = new Chapters();

            return Task.FromResult(chapters);
        }

        public Task<List<Verses>> GetApiScripture(string translationId, int bookId, int chapter)
        {
            List<Verses> verses = new List<Verses>();

            return Task.FromResult(verses);
        }


        private async Task<List<Translations>?> GetDatabaseTranslations(string userId)
        {
            try
            {
                string cacheKey = CacheKeys.Translations(userId);
                var cachedTranslations = await GetCachedData<List<Translations>?>(cacheKey);

                if (cachedTranslations != null)
                    return cachedTranslations;

                return await GetAvailableTranslations(cacheKey);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve database translations:");
                return null;
            }
        }

        private async Task<List<Books>?> GetDatabaseBooks(string translationId, string cacheKey)
        {
            List<Books> books = new List<Books>();

            var translation = await GetTranslation(translationId);

            if (string.IsNullOrEmpty(translation?.Id))
                return null;

            if (translation.UsesApi)
                return await GetApiBooks(translationId);

            books = await _dbContext.Books
                .AsNoTracking()
                .Where(b => b.TranslationId == translationId)
                .OrderBy(b => b.Id)
                .ToListAsync();

            if (books.Count > 0)
                await SetCachedData<List<Books>>(cacheKey, books);

            return books;
        }

        private async Task<List<BookInfo>?> GetDatabaseTranslationBibleBooks(string translationId, string cacheKey)
        {
            try
            {
                List<BookInfo> books = new List<BookInfo>();

                var translation = await GetTranslation(translationId);

                if (string.IsNullOrEmpty(translation?.Id))
                    return null;

                if (translation.UsesApi)
                    return await GetApiTranslationBibleBooks(translationId);

                if (books.Count > 0)
                    await SetCachedData<List<BookInfo>>(cacheKey, books);

                return books;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve translation Bible books:");
                return null;
            }
        }

        private async Task<List<Chapters>?> GetDatabaseAllChapters(string translationId, string cacheKey)
        {
            List<Chapters> chapters = new List<Chapters>();

            var translation = await GetTranslation(translationId);

            if (string.IsNullOrEmpty(translation?.Id))
                return null;

            if (translation.UsesApi)
                return await GetApiAllChapters(translationId);

            //get the distinct count of chapters for each bible chapter
            var allChapters = _dbContext.Verses
                .Where(v => v.TranslationId == translationId)
                .Join(
                    _dbContext.Books,
                    verses => new { verses.BookId, verses.TranslationId },
                    books => new { books.BookId, books.TranslationId },
                    (verses, books) => new { verses, books }
                )
                .GroupBy(composite => new
                {
                    composite.books.Id,
                    composite.verses.TranslationId,
                    composite.books.Name,
                    composite.books.Abbreviation
                })
                .OrderBy(g => g.Key.Id)
                .Select(group => new
                {   
                    BookId = group.Key.Id,
                    TranslationId = group.Key.TranslationId,
                    Name = group.Key.Name,
                    Abbreviation = group.Key.Abbreviation,
                    ChapterCount = group.Select(v => v.verses.Chapter).Distinct().Count()
                })                
                .ToList();


            if (allChapters.Count > 0)
            {
                foreach (var chapter in allChapters)
                {
                    var chapterNumbers = Enumerable.Range(1, chapter.ChapterCount).ToArray();

                    var addChapter = new Chapters
                    {
                        TranslationId = chapter.TranslationId,
                        BookName = chapter.Name,
                        BookAbbreviation = chapter.Abbreviation,
                        ChapterNumbers = chapterNumbers
                    };

                    chapters.Add(addChapter);
                }

                if (chapters.Count > 0)
                    await SetCachedData<List<Chapters>>(cacheKey, chapters);
            }

            return chapters;
        }

        private async Task<Chapters?> GetDatabaseChapters(string translationId, int bookId, string cacheKey)
        {
            Chapters chapters = new Chapters();

            var translation = await GetTranslation(translationId);

            if (string.IsNullOrEmpty(translation?.Id))
                return null;

            if (translation.UsesApi)
                return await GetApiChapters(translationId, bookId);

            var bookverses = await _dbContext.Verses
                .Where(v => v.TranslationId == translationId && v.BookId == bookId)
                .Select(v => v.Chapter)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            if (bookverses.Count() > 0)
            {
                chapters.TranslationId = translationId;
                chapters.ChapterNumbers = bookverses.ToArray();

                await SetCachedData<Chapters>(cacheKey, chapters);
            }

            return chapters;
        }

        private async Task<List<Translations>?> GetAvailableTranslations(string cacheKey)
        {
            try
            {
                List<Translations> translation = new List<Translations>();

                translation = await _dbContext.BibleTranslations
                    .AsNoTracking()
                    .ToListAsync();

                if (translation.Count > 0)
                    await SetCachedData<List<Translations>>(cacheKey, translation);

                return translation;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve the available translations:");
                return null;
            }
        }

        private async Task<Translations?> GetTranslation(string translationId)
        {
            try
            {
                Translations? translation = null;
                translation = await _dbContext.BibleTranslations.FirstOrDefaultAsync(t => t.Id == translationId);
                return translation;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve the translation:");
                return null;
            }
        }

        private async Task<List<Verses>?> GetDatabaseScripture(string translationId, int bookId, int chapter, string cacheKey)
        {
            List<Verses> verses = new List<Verses>();

            var translation = await GetTranslation(translationId);

            if (string.IsNullOrEmpty(translation?.Id))
                return null;

            if (translation.UsesApi)
                return await GetApiScripture(translationId, bookId, chapter);

            verses = await _dbContext.Verses
                .AsNoTracking()
                .Where(v => v.TranslationId == translationId && v.BookId == bookId && v.Chapter == chapter)
                .OrderBy(v => v.VerseNumber)
                .ToListAsync();

            if (verses.Count > 0)
                await SetCachedData<List<Verses>>(cacheKey, verses);

            return verses;
        }

        private async Task<T?> GetCachedData<T>(string cacheKey)
        {
            try
            {
                return await _cacheService.GetAsync<T>(cacheKey);
            }
            catch(RedisConnectionException ex)
            {
                _logger.LogError(ex, @$"Redis connection failed while reading key ${cacheKey}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, @$"Failed to retrieve Redis cached data for {cacheKey}. Still attempted to remove Redis cached data for {cacheKey}:");
                await _cacheService.RemoveAsync(cacheKey);
                return default;
            }
        }

        private async Task SetCachedData<T>(string cacheKey, T data)
        {
            await _cacheService.SetAsync(cacheKey, data, TimeSpan.FromHours(24));
        }





        private UserContext GetTemporaryUser()
        {
            UserContext userContext = new UserContext
            {
                UserId = "1234-5678-9987-6543",
                Email = "test.user@test.com",
                DisplayName = "TestUser7"
            };

            return userContext;
        }


        public async Task FixSqlFile()
        {
            string folder = @"C:\code\scripture hub\sql";

            string inputFile = Path.Combine(folder, "add_world_english_bible.sql");
            string outputFile = Path.Combine(folder, "add_world_english_bible_fixed.sql");

            var outputLines = new List<string>();

            foreach (string line in File.ReadLines(inputFile))
            {
                if (!line.StartsWith("INSERT INTO [Verses]", StringComparison.OrdinalIgnoreCase))
                {
                    outputLines.Add(line);
                    continue;
                }

                string updatedLine = FixVerseInsert(line);
                outputLines.Add(updatedLine);
            }

            File.WriteAllLines(outputFile, outputLines);

            Console.WriteLine($"Finished. Output written to {outputFile}");
        }

        private static string FixVerseInsert(string line)
        {
            Match match = Regex.Match(
                line,
                @"VALUES\s*\((.*)\);",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return line;

            string valuesText = match.Groups[1].Value;

            List<string> values = SplitSqlValues(valuesText);

            // Only modify rows that contain 6 values
            if (values.Count != 6)
                return line;

            // Remove the first numeric value after translation_id
            values.RemoveAt(1);

            string newValues = string.Join(", ", values);

            return Regex.Replace(
                line,
                @"VALUES\s*\((.*)\);",
                $"VALUES ({newValues});",
                RegexOptions.IgnoreCase);
        }

        private static List<string> SplitSqlValues(string input)
        {
            var values = new List<string>();
            var current = new StringBuilder();

            bool inString = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '\'')
                {
                    current.Append(c);

                    // Handle escaped SQL quotes ('')
                    if (inString && i + 1 < input.Length && input[i + 1] == '\'')
                    {
                        current.Append(input[i + 1]);
                        i++;
                        continue;
                    }

                    inString = !inString;
                    continue;
                }

                if (c == ',' && !inString)
                {
                    values.Add(current.ToString().Trim());
                    current.Clear();
                    continue;
                }

                current.Append(c);
            }

            if (current.Length > 0)
            {
                values.Add(current.ToString().Trim());
            }

            return values;
        }


    }
}
