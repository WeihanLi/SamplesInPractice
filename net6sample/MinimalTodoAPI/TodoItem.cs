using System.ComponentModel.DataAnnotations;

namespace MinimalTodoAPI;

public class TodoItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(256)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool Done { get; set; }

    public DateTime CreatedAt { get; set; }
}
