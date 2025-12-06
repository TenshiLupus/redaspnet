using System;

namespace booksBackend.Models;

public class QuoteDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }

    public bool isFavorite { get; set; }
}
