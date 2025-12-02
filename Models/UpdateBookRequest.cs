using System;

namespace booksBackend.Models;

public class UpdateBookRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishedDate { get; set; }
}