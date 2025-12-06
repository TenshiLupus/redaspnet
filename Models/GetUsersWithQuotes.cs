using System;

namespace booksBackend.Models;

public class GetUsersWithQuotes
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public List<QuoteDto> Quotes { get; set; } = new List<QuoteDto>();
    public List<BookDto> Books { get; set; } = new List<BookDto>();
}

