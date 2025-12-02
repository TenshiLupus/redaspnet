using System;

namespace booksBackend.Models;

public class GetAllQuotesRequest
{
    public int? Page { get; set; }
    public int? RecordsPerPage { get; set; }

    public string? DescriptionContains { get; set; }
    public string? AuthorContains { get; set; }
}