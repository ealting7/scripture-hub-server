namespace scripture_hub_server.Application.Constants
{
    public class CacheKeys
    {

        public static string Translations(string userId)
        {
            return $"translations:translations_for_user:{userId}";
        }

        public static string Scripture(string translationId, int bookId, int chapter)
        {
            return $"scripture:{translationId}:{bookId}:{chapter}";
        }

        public static string BookInfo(string translationId)
        {
            return $"bookInfo:{translationId}";
        }

        public static string Books(string translationId)
        {
            return $"books:{translationId}";
        }

        public static string Chapters(string translationId, int bookId)
        {
            return $"chapters:{translationId}:{bookId}";
        }

        public static string AllChapters(string translationId)
        {
            return $"allChapters:{translationId}";
        }
    }
}
