using Newtonsoft.Json;

namespace WebApp
{
    public class ApiClient(HttpClient httpClient)
    {
        public async Task<T1> PostAsync<T1, T2>(string url, T2 data)
        {
            var response = await httpClient.PostAsJsonAsync(url, data);
            if(response!=null && response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<T1>(await response.Content.ReadAsStringAsync());
                return result;
            }
            return default;
        }
    }
}
