using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using ReceiptApp.Services;

namespace ReceiptApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient
            {
                //BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                BaseAddress = new Uri(builder.Configuration["Api:Uri"] ?? throw new ArgumentNullException("Invalid API Uri in configuration"))
            });

            builder.Services.AddSingleton<WebSocketApiConfig>(new WebSocketApiConfig{Uri= builder.Configuration["WebSocketApi:Uri"] ?? throw new ArgumentNullException("Invalid Websocket API Uri in configuration") } );

            builder.Services.AddTransient<TokenService>();
            builder.Services.AddTransient<ReceiptApi>();
            builder.Services.AddSingleton<ReceiptExport>();

            builder.Services.AddTransient<ReceiptQuery>();
            builder.Services.AddTransient<ReceiptCommand>();
            builder.Services.AddTransient<JobsQuery>();
            builder.Services.AddTransient<UploaderService>();

            builder.Services.AddScoped<ReceiptLoaderStateService>();

            builder.Services.AddOidcAuthentication(options =>
            {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("Local", options.ProviderOptions);
            });


            builder.Services.AddSingleton(new Configuration.AuthenticationConfig(
                builder.Configuration["Local:Authority"] ?? throw new ArgumentNullException("Local:Authority"),
                builder.Configuration["Local:ClientId"] ?? throw new ArgumentNullException("Local:ClientId")));

            builder.Services.AddBlazorise(options =>
            {
                options.Immediate = true;
            }).AddBootstrapProviders().AddFontAwesomeIcons();

            await builder.Build().RunAsync();
        }
    }

    public class WebSocketApiConfig
    {
        public string Uri { get; set; } = "";
    }
}
