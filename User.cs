
namespace booksBackend;

public class User
{
    public int Id {get; set;}
    public required string Username {get; set;}
    public required string Password {get; set;}

    public List<UserBook> Books {get; set;} = new List<UserBook>();
    public List<UserQuote> Quotes {get; set;} = new List<UserQuote>();
}

public class UserBook
{
    public int Id {get; set;}
    public int UserId {get; set;}
    public User User {get; set;} = null!;

    public int BookId {get; set;}

    public Book Book {get; set;} = null!;

}

public class UserQuote
{
    public int Id {get; set;}
    public int UserId {get; set;}
    public User User {get; set;} = null!;

    public int QuoteId {get; set;}

    public Quote Quote {get; set;} = null!;

}