using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace WebApp
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthStateProvider _customAuthStateProvider;

        // Constructor injection of HttpClient and AuthenticationStateProvider
        public ApiClient(HttpClient httpClient, CustomAuthStateProvider customAuthStateProvider)
        {
            _httpClient = httpClient;
            _customAuthStateProvider = customAuthStateProvider;
        }
        // Dynamically set the Authorization header using token from the cookie
        private async Task setAuthorizedHeader()
        {
            var token = _customAuthStateProvider.GetTokenFromCookie(); // Get token from AuthenticationStateProvider
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T1> PostAsync<T1, T2>(string url, T2 data)
        {
            await setAuthorizedHeader();
            var response = await _httpClient.PostAsJsonAsync(url, data);
            if(response!=null && response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<T1>(await response.Content.ReadAsStringAsync());
                return result;
            }
            return default;
        }
        public async Task<T> GetFromJsonAsync<T>(string path)
        {
            await setAuthorizedHeader();
            return await _httpClient.GetFromJsonAsync<T>(path);
        }
        public async Task<T1> PutAsync<T1, T2>(string path, T2 postModel)
        {
            await setAuthorizedHeader();
            var res = await _httpClient.PutAsJsonAsync(path, postModel);
            if (res != null && res.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T1>(await res.Content.ReadAsStringAsync());
            }
            return default;
        }
        public async Task<T> DeleteAsync<T>(string path)
        {
            await setAuthorizedHeader();
            return await _httpClient.DeleteFromJsonAsync<T>(path);
        }
    }
}
