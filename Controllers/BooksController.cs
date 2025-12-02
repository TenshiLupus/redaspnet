using System.Security.Claims;
using booksBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace booksBackend.Controllers;

public class BooksController : BaseController
{
    private readonly ILogger<UsersController> _logger;
    private readonly AppDbContext _dbContext;
    public BooksController(ILogger<UsersController> logger, AppDbContext dbContext)
    {
        _logger = logger;
        this._dbContext = dbContext;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllBooks([FromQuery] GetAllBooksRequest request)
    {
        int page = request?.Page ?? 1;
        int numberOfRecords = request?.RecordsPerPage ?? 100;

        IQueryable<Book> query = _dbContext.Books

            .Skip((page - 1) * numberOfRecords)
            .Take(numberOfRecords);

        if (request != null)
        {
            if (!string.IsNullOrWhiteSpace(request.TitleContains))
            {
                query = query.Where(b => b.Title.Contains(request.TitleContains));
            }

            if (!string.IsNullOrWhiteSpace(request.AuthorContains))
            {
                query = query.Where(b => b.Author.Contains(request.AuthorContains));
            }

            // if (request.UserId is not null)
            // {
            //     query = query.Where(b => b.UserId == request.UserId);
            // }
        }

        var books = await query.ToArrayAsync();


        return Ok(books);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBookById([FromRoute] int id)
    {
        var existingBook = await _dbContext.Books.FindAsync(id);

        if (existingBook == null)
        {
            return NotFound();
        }

        return Ok(existingBook);
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateBook([FromRoute] int id, [FromBody] UpdateBookRequest request)
    {
        _logger.LogInformation("Updating book with ID: {BookId}", id);

        var existingBook = await _dbContext.Books.FindAsync(id);
        if (existingBook == null)
        {
            _logger.LogWarning("Book with ID: {BookId} not found", id);
            return NotFound();
        }

        if (request.Title is not null)
            existingBook.Title = request.Title;

        if (request.Author is not null)
            existingBook.Author = request.Author;

        if (request.PublishedDate is not null)
            existingBook.PublishedDate = request.PublishedDate.Value;

        try
        {
            _dbContext.Entry(existingBook).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Book with ID: {BookId} successfully updated", id);
            return Ok(existingBook);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while updating the book with ID: {BookId}", id);
            return StatusCode(500, "An error occurred while updating the book");
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        _logger.LogInformation("Deleting book with ID: {BookId}", id);

        var book = await _dbContext.Books.FindAsync(id);

        if (book == null)
        {
            _logger.LogWarning("Book with ID: {BookId} not found", id);
            return NotFound();
        }

        try
        {
            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Book with ID: {BookId} successfully deleted", id);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while deleting the book with ID: {BookId}", id);
            return StatusCode(500, "An error occurred while deleting the book");
        }
    }


    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Book), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
       
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); 
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Forbid(); // token missing / invalid user id
        }

        
        var newBook = new Book
        {
            Title = request.Title,
            Author = request.Author,
            PublishedDate = (DateTime)request.PublishedDate!,
            Image = request.Image
        };

        try
        {
            
            _dbContext.Books.Add(newBook);
            await _dbContext.SaveChangesAsync(); 

            
            var userBook = new UserBook
            {
                UserId = userId,
                BookId = newBook.Id
            };

            _dbContext.UserBooks.Add(userBook);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Book with ID: {BookId} successfully created for user {UserId}", newBook.Id, userId);

            
            return CreatedAtAction(nameof(GetBookById), new { id = newBook.Id }, newBook);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while creating new book");
            return StatusCode(500, "An error occurred while creating the book");
        }
    }
}
