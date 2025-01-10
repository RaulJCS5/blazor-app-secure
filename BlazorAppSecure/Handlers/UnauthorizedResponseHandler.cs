using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorAppSecure.Handlers
{
    public class UnauthorizedResponseHandler : DelegatingHandler
    {
        private readonly NavigationManager _navigationManager;

        public UnauthorizedResponseHandler(NavigationManager navigationManager, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var currentUri = _navigationManager.Uri;
                var loginUri = _navigationManager.BaseUri + "login";

                if (!currentUri.Equals(loginUri, System.StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        _navigationManager.NavigateTo("/", true);
                    }
                    catch (NavigationException)
                    {
                        
                    }
                }
            }

            return response;
        }
    }
}