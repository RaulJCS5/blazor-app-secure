using BlazorAppIdentity.Model;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BlazorAppIdentity.Services
{
    public class BlogService : IBlogService
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly HttpClient _httpClient;
        public BlogService()
        {
            
        }

        public BlogService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Auth");
        }

        public async Task<BlogViewModel> CreateAsync(BlogViewModel blogViewModel)
        {
            var blog = await _httpClient.PostAsJsonAsync<BlogViewModel>("api/blog", blogViewModel);
            var blogObject = await blog.Content.ReadAsStringAsync();
            var createBlog = JsonSerializer.Deserialize<BlogViewModel>(blogObject, jsonSerializerOptions);
            return createBlog;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _httpClient.DeleteAsync($"api/blog/{id}");
            return result.IsSuccessStatusCode;
        }

        public async Task<List<BlogViewModel>> GetAllAsync()
        {
            var blogs = await _httpClient.GetFromJsonAsync<List<BlogViewModel>>("api/blog");
            return blogs;
        }

        public async Task<BlogViewModel> GetByIdAsync(int id)
        {
            var blog = await _httpClient.GetFromJsonAsync<BlogViewModel>($"api/blog/{id}");
            return blog;
        }

        public async Task<bool> UpdateAsync(int id, BlogViewModel blog)
        {
            var emptyContent = new StringContent(JsonSerializer.Serialize(blog), 
                Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/blog/{id}", emptyContent);
            return response.IsSuccessStatusCode;
        }
    }
}
