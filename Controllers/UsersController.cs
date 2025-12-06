using System.Threading.Tasks;
using booksBackend.Models;
using booksBackend.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace booksBackend.Controllers
{

    public class UsersController : BaseController
    {

        private readonly ILogger<UsersController> _logger;
        private readonly AppDbContext _dbContext;
        public UsersController(ILogger<UsersController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            this._dbContext = dbContext;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetUsersWithQuotes>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersRequest request)
        {
            int page = request?.Page ?? 1;
            int numberOfRecords = request?.RecordsPerPage ?? 100;

            IQueryable<User> query = _dbContext.Users
                .Skip((page - 1) * numberOfRecords)
                .Take(numberOfRecords);

            if (request != null && !string.IsNullOrWhiteSpace(request.UsernameContains))
            {
                query = query.Where(e => e.Username.Contains(request.UsernameContains));
            }

            var users = await query
                .Select(u => new GetUsersWithQuotes
                {
                    Id = u.Id,
                    Username = u.Username,

                    Quotes = u.Quotes.Select(uq => new QuoteDto
                        {
                            Id = uq.Quote.Id,
                            Description = uq.Quote.Description,
                            Author = uq.Quote.Author,
                            isFavorite = uq.Quote.isFavorite,
                        })
                        .ToList(),

                    Books = u.Books.Select(b => new BookDto
                    {
                        Id = b.Book.Id,
                        Title = b.Book.Title,
                        Author = b.Book.Author,
                        PublishedDate = b.Book.PublishedDate
                    }).ToList()
                })
                .ToListAsync();

            return Ok(users);

        }

        [Authorize]
        [HttpGet("favoriteQuotes")]
        [ProducesResponseType(typeof(IEnumerable<GetUsersWithQuotes>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersFavoriteQuotes([FromQuery] GetAllUsersRequest request)
        {
            int page = request?.Page ?? 1;
            int numberOfRecords = request?.RecordsPerPage ?? 100;

            IQueryable<User> query = _dbContext.Users
                .Skip((page - 1) * numberOfRecords)
                .Take(numberOfRecords);

            if (request != null && !string.IsNullOrWhiteSpace(request.UsernameContains))
            {
                query = query.Where(e => e.Username.Contains(request.UsernameContains));
            }

            var users = await query
                .Select(u => new GetUsersWithQuotes
                {
                    Id = u.Id,
                    Username = u.Username,

                    Quotes = u.Quotes
                        .Where(uq => uq.Quote.isFavorite)
                        .OrderBy(uq => uq.Quote.Id)
                        .Take(5)
                        .Select(uq => new QuoteDto
                        {
                            Id = uq.Quote.Id,
                            Description = uq.Quote.Description,
                            Author = uq.Quote.Author,
                            isFavorite = uq.Quote.isFavorite,
                        })
                        .ToList(),

                    Books = u.Books.Select(b => new BookDto
                    {
                        Id = b.Book.Id,
                        Title = b.Book.Title,
                        Author = b.Book.Author,
                        PublishedDate = b.Book.PublishedDate
                    }).ToList()
                })
                .ToListAsync();

            return Ok(users);

        }

        /// <summary>
        /// Gets a book by ID.
        /// </summary>
        /// <param name="id">The ID of the book.</param>
        /// <returns>The single book record.</returns>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(e => e.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userResponse = UserToUserResponse(user);

            return Ok(userResponse);
        }

        private static GetUsersResponse UserToUserResponse(User user)
        {
            return new GetUsersResponse
            {
                Username = user.Username,
                // UserQuotes = user.UserQuotes.Select(QuoteToQuoteResponse).ToList()
            };
        }



        private static GetUserResponseUserQuote QuoteToQuoteResponse(Quote quote)
        {
            return new GetUserResponseUserQuote
            {
                Id = quote.Id,
                Description = quote.Description,
                Author = quote.Author
            };
        }

        [Authorize]
        [HttpGet("{userId}/books")]
        [ProducesResponseType(typeof(IEnumerable<GetUserResponseUserBook>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooksForUser(int userId)
        {
            var user = await _dbContext.Users
            .Include(user => user.Books)
            .ThenInclude(e => e.Book)
            .FirstOrDefaultAsync(e => e.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var books = user.Books.Select(b => new GetUserResponseUserBook
            {
                Id = b.Id,
                Title = b.Book.Title,
                Author = b.Book.Author,
                PublishedDate = b.Book.PublishedDate

            });

            return Ok(books);
        }


        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="userRequest">The book to be created.</param>
        /// <returns>A link to the book that was created.</returns>

        [HttpPost]
        [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest userRequest)
        {
            var newUser = new User
            {
                Username = userRequest.Username,
                Password = userRequest.Password,
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }

        [Authorize]
        [HttpGet("{userId}/quotes")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQuotesForUser(int userId)
        {
            var user = await _dbContext.Users
            .Include(user => user.Quotes)
            .ThenInclude(e => e.Quote)
            .FirstOrDefaultAsync(e => e.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var books = user.Quotes.Select(q => new QuoteDto
            {
                Id = q.Id,
                Description = q.Quote.Description,
                Author = q.Quote.Author,
                isFavorite = q.Quote.isFavorite

            });

            return Ok(books);
        }

        //look at automapper for property assignment

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="id">The ID of the book to update.</param>
        /// <param name="userRequest">The book data to update.</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UpdateUserRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, UpdateUserRequest userRequest)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);
            var existingUser = await _dbContext.Users.FindAsync(id);

            if (existingUser == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found", id);
                return NotFound();
            }

            _logger.LogDebug("Updating User details for ID: {UserId}", id);
            if (userRequest.Username is not null)
                existingUser.Username = userRequest.Username;

            if (userRequest.Password is not null)
                existingUser.Password = userRequest.Password;

            // if (userRequest.UserBooks is not null)
            //     existingUser.UserBooks = userRequest.UserBooks;

            // if (userRequest.UserQuotes is not null)
            //     existingUser.UserQuotes = userRequest.UserQuotes;

            try
            {
                _dbContext.Entry(existingUser).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("User with ID: {UserId} successfully updated", id);
                return Ok(existingUser);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while update the User with ID: {UserId}", id);
                return StatusCode(500, "An error ocurred while updating the User");
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
