using System.Text.Json.Serialization;

namespace ReceiptApp.Model
{
    public class JobsResult
    {
        public IEnumerable<Job> Jobs { get; set; } = new List<Job>();
    }

    public class Job
    {
        [JsonPropertyName("job_id")]
        public string? JobId { get; set; }
        public string? Status { get; set; }
        [JsonPropertyName("submission_time")]
        public string? SubmissionTime { get; set; }
        [JsonPropertyName("completed_time")]
        public string? CompletedTime { get; set; }
        [JsonPropertyName("file_path")]
        public string? FilePath { get; set; }
    }
}