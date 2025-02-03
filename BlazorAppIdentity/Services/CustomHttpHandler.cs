using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazorAppIdentity.Services
{
    public class CustomHttpHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add custom logic here
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            request.Headers.Add("X-Requested-With", ["XMLHttpRequest"]);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
