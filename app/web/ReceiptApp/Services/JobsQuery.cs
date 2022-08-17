using ReceiptApp.Model;

namespace ReceiptApp.Services
{
    public class JobsQuery
    {
        private const string jobsUrl = "jobs";
        private readonly ReceiptApi _api;
        public JobsQuery(ReceiptApi api)
        {
            _api = api;
        }

        public async Task<IEnumerable<Job>> GetAll()
        {
            return (await _api.Get<JobsResult>(jobsUrl))?.Jobs ?? new List<Job>();
        }
    }
}