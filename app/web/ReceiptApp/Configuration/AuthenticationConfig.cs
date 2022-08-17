namespace ReceiptApp.Configuration
{
    public class AuthenticationConfig
    {
        public AuthenticationConfig(string authority, string clientId)
        {
            Authority = authority
                ?? throw new ArgumentNullException(nameof(authority));
            ClientId = clientId
                ?? throw new ArgumentNullException(nameof(clientId));
        }

        public string Authority { get; }
        public string ClientId { get; }
    }
}