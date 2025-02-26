using identitywebapiauthentication.Data;
using identitywebapiauthentication.Entity;
using Microsoft.EntityFrameworkCore;

namespace identitywebapiauthentication.Services
{
    public class BlogService : IBlogService
    {
        private readonly IdentityDbContext _dbContext;
        public BlogService(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Blog> CreateAsync(Blog blog)
        {
            await _dbContext.Blogs.AddAsync(blog);
            await _dbContext.SaveChangesAsync();
            return blog;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var blog = await _dbContext.Blogs.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (blog != null)
            {
                _dbContext.Blogs.Remove(blog);
                await _dbContext.SaveChangesAsync();
                id = blog.Id;
            }
            return blog?.Id > 0;
        }

        public async Task<List<Blog>> GetAllAsync()
        {
            var blogList = await _dbContext.Blogs.ToListAsync();
            return blogList;
        }

        public async Task<Blog> GetByIdAsync(int id)
        {
            var blog = await _dbContext.Blogs.Where(x => x.Id == id).FirstOrDefaultAsync();
            return blog;
        }

        public async Task<bool> UpdateAsync(int id, Blog blog)
        {
            var exisitingBlog = await _dbContext.Blogs.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (exisitingBlog != null)
            {
                exisitingBlog.Name = blog.Name;
                exisitingBlog.Description = blog.Description;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
