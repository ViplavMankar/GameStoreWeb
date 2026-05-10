using System;
using System.ComponentModel.DataAnnotations;

namespace GameStoreWeb.Models;

public class Blog
{
    [Key]
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public Guid? AuthorUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
