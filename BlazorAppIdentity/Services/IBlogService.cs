using BlazorAppIdentity.Model;

namespace BlazorAppIdentity.Services
{
    public interface IBlogService
    {
        Task<List<BlogViewModel>> GetAllAsync();
        Task<BlogViewModel> GetByIdAsync(int id);
        Task<BlogViewModel> CreateAsync(BlogViewModel BlogViewModel);
        Task<bool> UpdateAsync(int id, BlogViewModel blog);
        Task<bool> DeleteAsync(int id);
    }
}
