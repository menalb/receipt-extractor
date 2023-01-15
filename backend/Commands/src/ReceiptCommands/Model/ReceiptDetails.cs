using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ReceiptCommand.Model
{
    public class ReceiptDetails
    {
        [JsonPropertyName("id")] 
        public string Id { get; set; }
        [JsonPropertyName("job_id")]
        public string JobId { get; set; }
        [JsonPropertyName("day")]
        public string Day { get; set; }
        [JsonPropertyName("shop")]
        public Shop Shop { get; set; }
        [JsonPropertyName("total")]
        public decimal? Total { get; set; }
        [JsonPropertyName("total_vat")]
        public decimal? TotalVAT { get; set; }
        [JsonPropertyName("items")]
        public IEnumerable<ReceiptItem> Items { get; set; }
        [JsonPropertyName("tags")]
        public IEnumerable<string> Tags { get; set; }
        [JsonPropertyName("notes")]
        public string Notes { get; set; }
    }

    public class Shop
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("owner")]
        public string Owner { get; set; }
        [JsonPropertyName("vat")]
        public string Vat { get; set; }
        [JsonPropertyName("phone")]
        public string Phone { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("city")]
        public string City { get; set; }
    }

    public class ReceiptItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("vat")]
        public decimal? VAT { get; set; }
        [JsonPropertyName("price")]
        public decimal? Price { get; set; }
    }
}