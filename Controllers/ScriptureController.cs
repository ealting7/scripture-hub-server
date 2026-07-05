using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using scripture_hub_server.Application.Interfaces;

namespace scripture_hub_server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ScriptureController : ControllerBase
    {
        private readonly IBibleService _bibleService;
        public ScriptureController(IBibleService bibleService) 
        {
            _bibleService = bibleService;
        }

        [HttpGet("translations")]
        public async Task<IActionResult> GetBibleTranslations()
        {
            var translations = await _bibleService.GetBibleTranslation();

            if (translations == null)
            {
                return NotFound();
            }

            return Ok(translations);
        }


        [HttpGet("translations/{translationId}/books")]
        public async Task<IActionResult> GetBibleTranslationBookInfo(string translationId)
        {
            var translations = await _bibleService.GetTranslationBookInfo(translationId);
            return Ok(translations);
        }

        [HttpGet("books/{translationId}")]
        public async Task<IActionResult> GetBibleTranslationBooks(string translationId)
        {
            var books = await _bibleService.GetBooksForTranslation(translationId);
            return Ok(books);
        }


        [HttpGet("books/{translationId}/chapters")]
        public async Task<IActionResult> GetBibleBooksChapters(string translationId)
        {
            var books = await _bibleService.GetAllChaptersForAllBooks(translationId);
            return Ok(books);
        }

        [HttpGet("chapters/{translationId}/{bookId}")]
        public async Task<IActionResult> GetBibleTranslationChapters(string translationId, int bookId)
        {
            var chapters = await _bibleService.GetChaptersForTranslation(translationId, bookId);
            return Ok(chapters);
        }

        [HttpGet("{translationId}/{bookId}/{chapter}")]
        public async Task<IActionResult> GetBibleTranslationScripture(string translationId, int bookId, int chapter)
        {
            var scripture = await _bibleService.GetScriptureForTranslation(translationId, bookId, chapter);
            return Ok(scripture);
        }







        [HttpGet("fixsql")]
        public async Task<IActionResult> FixSql()
        {
            await _bibleService.FixSqlFile();
            return Ok("Completed");
        }



    }
}
