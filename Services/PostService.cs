using BlogApp.Models;
using BlogApp.Models.BlogApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Services
{
    public interface IPostService
    {
        Task DeletePostAfter24Hours(int postId);
    }

    public class PostService : IPostService
    {
        private readonly BlogContext _context;

        public PostService(BlogContext context)
        {
            _context = context;
        }

        public async Task DeletePostAfter24Hours(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}