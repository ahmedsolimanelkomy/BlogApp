namespace BlogApp.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;
        public List<Comment> Comments { get; set; } = [];
        public List<PostTag> PostTags { get; set; } = [];
    }
}
