using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

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
                _navigationManager.NavigateTo("/login", true);
            }

            return response;
        }
    }
}