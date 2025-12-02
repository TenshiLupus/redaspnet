using System;

namespace booksBackend;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<UserQuote> UserQuotes { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<UserBook> UserBooks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<UserQuote>().HasIndex(q => new {q.QuoteId, q.UserId}).IsUnique();
        modelBuilder.Entity<UserBook>().HasIndex(q => new {q.BookId, q.UserId}).IsUnique();
    }
}
