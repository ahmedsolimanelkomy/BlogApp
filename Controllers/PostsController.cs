using BlogApp.Models;
using BlogApp.Models.BlogApp.Models;
using BlogApp.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly BlogContext _context;
        private readonly IPostService _postService;

        public PostsController(BlogContext context, IPostService postService)
        {
            _context = context;
            _postService = postService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PostDto postDto)
        {
            if (postDto.Tags == null || !postDto.Tags.Any())
                return BadRequest("At least one tag is required");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var post = new Post
            {
                Title = postDto.Title,
                Body = postDto.Body,
                AuthorId = userId,
                CreatedAt = DateTime.UtcNow,
                PostTags = postDto.Tags.Select(t => new PostTag { Tag = new Tag { Name = t } }).ToList()
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            BackgroundJob.Schedule(() => _postService.DeletePostAfter24Hours(post.Id), TimeSpan.FromHours(24));
            return CreatedAtAction(nameof(Get), new { id = post.Id }, post);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PostDto postDto)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (post.AuthorId != userId) return Forbid();

            post.Title = postDto.Title;
            post.Body = postDto.Body;
            if (postDto.Tags != null && postDto.Tags.Any())
            {
                _context.PostTags.RemoveRange(_context.PostTags.Where(pt => pt.PostId == id));
                post.PostTags = postDto.Tags.Select(t => new PostTag { Tag = new Tag { Name = t } }).ToList();
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (post.AuthorId != userId) return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
