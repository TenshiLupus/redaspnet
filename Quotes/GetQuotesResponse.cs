using System;

namespace booksBackend.Quotes;

public class GetQuotesResponse
{
    public int Id {get; set;}

    public required int UserId {get; set;}

    public required string Description {get; set;}
}
