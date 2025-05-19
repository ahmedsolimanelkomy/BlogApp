namespace BlogApp.DTO
{
    public class PostDto
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = [];
    }
}
