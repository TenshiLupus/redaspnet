using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace booksBackend;

public static class SeedData
{
    public static void MigrateAndSeed(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

       
        context.Database.Migrate();

        if (!context.Users.Any())
        {

            var u1 = new User { Username = "Taniks", Password = string.Empty };
            var u2 = new User { Username = "Zavala", Password = string.Empty };

            var hasher = new PasswordHasher<User>();
            u1.Password = hasher.HashPassword(u1, "cayde6");
            u2.Password = hasher.HashPassword(u2, "towers");


            context.Users.AddRange(u1, u2);
            context.SaveChanges();
        }

       
        if (!context.Books.Any())
        {
            var b1 = new Book { Title = "Fall", Author = "Oryx", PublishedDate = DateTime.Now };
            var b2 = new Book { Title = "Curse", Author = "Osiris", PublishedDate = DateTime.Now };
            var b3 = new Book { Title = "Felwinter", Author = "Peak", PublishedDate = DateTime.Now };
            var b4 = new Book { Title = "Saint", Author = "Osiris", PublishedDate = DateTime.Now };

            context.Books.AddRange(b1, b2, b3, b4);
            context.SaveChanges();
        }


        if (!context.Quotes.Any())
        {
            var q1 = new Quote { Description = "big team battle", Author = "Oryx" };
            var q2 = new Quote { Description = "killimanjaro", Author = "Osiris" };

            context.Quotes.AddRange(q1, q2);
            context.SaveChanges();
        }

    
        if (!context.UserBooks.Any())
        {
            var taniks = context.Users.Single(u => u.Username == "Taniks");
            var zavala = context.Users.Single(u => u.Username == "Zavala");

            var fall = context.Books.Single(b => b.Title == "Fall");
            var curse = context.Books.Single(b => b.Title == "Curse");
            var fel = context.Books.Single(b => b.Title == "Felwinter");
            var saint = context.Books.Single(b => b.Title == "Saint");

            context.UserBooks.AddRange(
               
                new UserBook { UserId = taniks.Id, BookId = fall.Id },
                new UserBook { UserId = taniks.Id, BookId = curse.Id },

            
                new UserBook { UserId = zavala.Id, BookId = fel.Id },
                new UserBook { UserId = zavala.Id, BookId = saint.Id }
            );

            context.SaveChanges();
        }

        if (!context.UserQuotes.Any())
        {
            var taniks = context.Users.Single(u => u.Username == "Taniks");
            var zavala = context.Users.Single(u => u.Username == "Zavala");

            var btb = context.Quotes.Single(q => q.Description == "big team battle");
            var kila = context.Quotes.Single(q => q.Description == "killimanjaro");

            context.UserQuotes.AddRange(
           
                new UserQuote { UserId = taniks.Id, QuoteId = btb.Id },
                new UserQuote { UserId = taniks.Id, QuoteId = kila.Id }


            );

            context.SaveChanges();

        }
    }
}
