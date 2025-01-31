using BlazorAppIdentity.Model;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorAppIdentity.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IAccountManagement
    {
        private bool _authenticated = false;

        private readonly ClaimsPrincipal Unauthenticated = new ClaimsPrincipal(new ClaimsIdentity());

        private readonly HttpClient _client;

        public CustomAuthenticationStateProvider(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("Auth");
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _authenticated = false;

            var user = Unauthenticated;
            
            return new AuthenticationState(user);
        }

        public async Task<FormResult> RegisterAsync(string email, string password)
        {
            string[] defaultDetail = ["An unkown error prevented registration from succeeding."];

            try
            {
                var result = await _client.PostAsJsonAsync("register", new { email, password });
                if (result.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                }
                var error = await result.Content.ReadAsStringAsync();
                var problemDetails = JsonDocument.Parse(error);
                var errors = new List<string>();
                var errorList = problemDetails.RootElement.GetProperty("errors");

                foreach (var item in errorList.EnumerateObject())
                {
                    if (item.Value.ValueKind == JsonValueKind.String)
                    {
                        errors.Add(item.Value.GetString());
                    }
                    else if (item.Value.ValueKind == JsonValueKind.Array)
                    {
                        errors.AddRange(item.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).Where(x => !string.IsNullOrWhiteSpace(x)));
                    }
                }
                return new FormResult
                {
                    Succeeded = false,
                    ErrorList = problemDetails == null ? defaultDetail : [.. errors]
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
