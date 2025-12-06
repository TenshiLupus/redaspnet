using System.Security.Claims;
using booksBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace booksBackend.Controllers;

public class QuotesController : BaseController
{
    private readonly ILogger<QuotesController> _logger;
    private readonly AppDbContext _dbContext;

    public QuotesController(ILogger<QuotesController> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }


    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Quote>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllQuotes([FromQuery] GetAllQuotesRequest request)
    {
        int page = request?.Page ?? 1;
        int numberOfRecords = request?.RecordsPerPage ?? 100;

        IQueryable<Quote> query = _dbContext.Quotes
            .Skip((page - 1) * numberOfRecords)
            .Take(numberOfRecords);

        if (request != null)
        {
            if (!string.IsNullOrWhiteSpace(request.DescriptionContains))
            {
                query = query.Where(q => q.Description.Contains(request.DescriptionContains));
            }

            if (!string.IsNullOrWhiteSpace(request.AuthorContains))
            {
                query = query.Where(q => q.Author.Contains(request.AuthorContains));
            }
        }

        var quotes = await query.ToArrayAsync();

        return Ok(quotes);
    }


    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Quote), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQuoteById([FromRoute] int id)
    {
        var existingQuote = await _dbContext.Quotes.FindAsync(id);

        if (existingQuote == null)
        {
            return NotFound();
        }

        return Ok(existingQuote);
    }


    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Quote), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateQuote([FromRoute] int id, [FromBody] QuoteDto request)
    {
        _logger.LogInformation("Updating quote with ID: {QuoteId}", id);

        var existingQuote = await _dbContext.Quotes.FindAsync(id);
        if (existingQuote == null)
        {
            _logger.LogWarning("Quote with ID: {QuoteId} not found", id);
            return NotFound();
        }

        _logger.LogDebug("Updating quote details for ID: {QuoteId}", id);

        if (request.Description is not null)
            existingQuote.Description = request.Description;

        if (request.Author is not null)
            existingQuote.Author = request.Author;

        existingQuote.isFavorite = request.isFavorite;


        try
        {
            _dbContext.Entry(existingQuote).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Quote with ID: {QuoteId} successfully updated", id);
            return Ok(existingQuote);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while updating the Quote with ID: {QuoteId}", id);
            return StatusCode(500, "An error occurred while updating the Quote");
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteQuote([FromRoute] int id)
    {
        _logger.LogInformation("Deleting quote with ID: {QuoteId}", id);

        var existingQuote = await _dbContext.Quotes.FindAsync(id);

        if (existingQuote == null)
        {
            _logger.LogWarning("Quote with ID: {QuoteId} not found", id);
            return NotFound();
        }

        try
        {
            _dbContext.Quotes.Remove(existingQuote);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Quote with ID: {QuoteId} successfully deleted", id);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while deleting the quote with ID: {QuoteId}", id);
            return StatusCode(500, "An error occurred while deleting the quote");
        }
    }


    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Quote), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequest request)
    {

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Forbid();
        }


        var newQuote = new Quote
        {

            Description = request.Description,
            Author = request.Author,
            isFavorite = false,

        };

        try
        {
            // add the quote to the context
            _dbContext.Quotes.Add(newQuote);
            await _dbContext.SaveChangesAsync();


            var userQuote = new UserQuote
            {
                UserId = userId,
                QuoteId = newQuote.Id,
            };

            _dbContext.UserQuotes.Add(userQuote);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Quote with ID: {QuoteId} successfully created for user {UserId}",
                newQuote.Id,
                userId
            );


            return CreatedAtAction(nameof(GetQuoteById), new { id = newQuote.Id }, newQuote);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while creating new quote");
            return StatusCode(500, "An error occurred while creating the quote");
        }
    }


    [Authorize]
    [HttpPatch("{id}/favorite")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetFavorite(
        int id,
        [FromBody] QuoteDto request)
    {

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("nameid");
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);


        var user = await _dbContext.Users
            .AsTracking()
            .Include(u => u.Quotes)
            .ThenInclude(uq => uq.Quote)
            .FirstOrDefaultAsync(u => u.Id == userId);


        // find the quote for this user that will be checked agains the existing favorite quotes prior to updating it
        Quote quote = user!.Quotes.FirstOrDefault(uq => uq.Quote.Id == id)!.Quote;


        //count checker
        if (request.isFavorite && !quote.isFavorite)
        {
            var currentFavoriteCount = user.Quotes.Count(uq => uq.Quote.isFavorite);
            if (currentFavoriteCount >= 5)
            {
                return BadRequest(new
                {
                    message = "You can only have up to 5 favorite quotes."
                });
            }
        }


        quote.isFavorite = request.isFavorite;
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            id = quote.Id,
            isFavorite = quote.isFavorite
        });
    }
}

