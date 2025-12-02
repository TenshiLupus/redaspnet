
namespace booksBackend.Books;

public class CreateBookRequest
{
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required DateTime PublishedDate { get; set; }

    public required int UserId { get; set; }

    public string? Image { get; set; }
}

public class GetBookResponse
{

    public required string Title { get; set; }
    public required string Author { get; set; }
    public required DateTime PublishedDate { get; set; }

    public string? Image { get; set; }
}


