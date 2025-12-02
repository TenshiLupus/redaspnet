using System;

namespace booksBackend.Models;

public class UpdateQuoteRequest
{
    public string? Description { get; set; }
    public string? Author { get; set; }
}
