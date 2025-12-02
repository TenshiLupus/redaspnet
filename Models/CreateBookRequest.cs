using System;

namespace booksBackend.Models;

public class CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime? PublishedDate { get; set; }
    public string? Image { get; set; }
}