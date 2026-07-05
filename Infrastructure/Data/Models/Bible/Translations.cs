using System.ComponentModel.DataAnnotations.Schema;

namespace scripture_hub_server.Infrastructure.Data.Models.Bible
{
    public class Translations
    {
        public required string Id { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? License { get; set; }
        public bool UsesApi { get; set; } = false;
        public string? ApiUrl { get; set; }
        public string Name { get; set; } = string.Empty;

        public int TypeId { get; set; } = 0;

        public string PaymentTier { get; set; } = string.Empty;
    }
}
