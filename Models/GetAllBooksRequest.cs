using System;

namespace booksBackend.Models;

public class GetAllBooksRequest
{
    public int? Page { get; set; }
    public int? RecordsPerPage { get; set; }
    public string? TitleContains { get; set; }
    public string? AuthorContains { get; set; }
}