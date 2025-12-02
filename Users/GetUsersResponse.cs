using System;
using booksBackend.Books;
using booksBackend.Quotes;
using Microsoft.Net.Http.Headers;

namespace booksBackend.Users;

public class GetUsersResponse
{

    public required string Username { get; set; }
    // public required List<GetUserResponseUserBook> UserBooks {get; set;}
    // public required List<GetUserResponseUserQuote> UserQuotes {get; set;}
}

public class GetUserResponseUserBook
{
    public int Id { get; set; }
    // public required int UserId { get; set; }

    public required string Title { get; set; }
    public required string Author { get; set; }
    public required DateTime PublishedDate { get; set; }

}

public class GetUserResponseUserQuote
{
    public int Id { get; set; }
    // public required int UserId { get; set; }
    public required string Description { get; set; }
    public required string Author { get; set; }

}

public class CreateUserRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }

    public List<Book>? UserBooks { get; set; }
    public List<Quote>? UserQuotes { get; set; }

}


