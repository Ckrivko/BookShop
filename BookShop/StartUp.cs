namespace BookShop
{
    using Data;
    using Initializer;
    using System.Linq;
    using System.Text;
    using System;
    using BookShop.Models.Enums;
    using System.Collections.Generic;
    using BookShop.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Globalization;
    using System.Diagnostics;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            // int inputInt = int.Parse(Console.ReadLine());
           
            Console.WriteLine(RemoveBooks(db));
           
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {

            AgeRestriction ageRestriction;

            bool isParseSuccsess =
                Enum.TryParse(command, true, out ageRestriction);

            if (!isParseSuccsess)
            {
                return String.Empty;
            }

            string[] books = context.Books
                .Where(b => b.AgeRestriction == ageRestriction)
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToArray();


            return String.Join(Environment.NewLine, books);

        }

        public static string GetGoldenBooks(BookShopContext context)
        {


            string[] books = context.Books
                    .Where(b => b.Copies < 5000 && b.EditionType == EditionType.Gold)
                    .OrderBy(b => b.BookId)
                    .Select(b => b.Title)
                    .ToArray();

            return String.Join(Environment.NewLine, books);
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            StringBuilder output = new StringBuilder();

            var books = context.Books
                .Where(b => b.Price > 40)
                .Select(b => new
                {
                    b.Title,
                    b.Price,
                })
                .OrderByDescending(b => b.Price)
                .ToArray();

            foreach (var b in books)
            {
                output.AppendLine($"{b.Title} - ${b.Price.ToString("F2")}");
            }


            return output.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {


            string[] books = context.Books

                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();



            return String.Join(Environment.NewLine, books);
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.ToLower())
                .ToArray();


            var books = context.Books
                 .Where(b => b.BookCategories.Any(b => categories.Contains(b.Category.Name.ToLower())))
                 .Select(b => b.Title)
                 .OrderBy(b => b)
                 .ToArray();



            string output = String.Join(Environment.NewLine, books);

            return output;

        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            StringBuilder output = new StringBuilder();

            var dateAsParsed = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);


            var books = context.Books
                .Where(b => b.ReleaseDate < dateAsParsed)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price,
                    b.ReleaseDate
                })
                .OrderByDescending(b => b.ReleaseDate)
                .ToArray();

            foreach (var b in books)
            {
                output.AppendLine($"{b.Title} - {b.EditionType} - ${b.Price.ToString("F2")}");
            }

            return output.ToString().TrimEnd();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            StringBuilder output = new StringBuilder();
            var authors = context.Authors
                .Where(a => a.FirstName.EndsWith(input))
               .Select(a => new
               {
                   a.FirstName,
                   a.LastName
               })
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .ToArray();

            foreach (var a in authors)
            {
                output.AppendLine($"{a.FirstName} {a.LastName}");
            }

            return output.ToString().TrimEnd();
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            string[] books = context.Books
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToArray();

            return String.Join(Environment.NewLine, books);
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            StringBuilder output = new StringBuilder();

            var books = context.Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .Select(b => new
                {
                    b.Title,
                    b.Author.FirstName,
                    b.Author.LastName,
                    b.BookId
                })
                .OrderBy(b => b.BookId)
                .ToArray();

            foreach (var b in books)
            {
                output.AppendLine($"{b.Title} ({b.FirstName} {b.LastName})");
            }

            return output.ToString().TrimEnd();
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {

            int output = context.Books
                    .Where(b => b.Title.Length > lengthCheck)
                    .Count();

            Console.WriteLine($"There are {output} books with longer title than {lengthCheck} symbols");

            return output;
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {

            StringBuilder output = new StringBuilder();

            var authors = context.Authors
                 .Select(a => new
                 {
                     a.FirstName,
                     a.LastName,
                     TotalCopies = a.Books.Sum(b => b.Copies)
                 })
                 .OrderByDescending(a => a.TotalCopies)
                 .ToArray();

            foreach (var a in authors)
            {
                output.AppendLine($"{a.FirstName} {a.LastName} - {a.TotalCopies}");
            }

            return output.ToString().TrimEnd();

        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            StringBuilder output = new StringBuilder();

            var categoryProfit = context.Categories
                .Select(c => new
                {
                    c.Name,
                    TotalProfit = c.CategoryBooks.Select(c => c.Book.Price * c.Book.Copies).Sum(a => a)
                })
                .OrderByDescending(t => t.TotalProfit)
                .ToArray();

            foreach (var c in categoryProfit)
            {
                output.AppendLine($"{c.Name} ${c.TotalProfit.ToString("f2")}");
            }


            return output.ToString().TrimEnd();

        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            StringBuilder output = new StringBuilder();

            var categoryRecentBooks = context.Categories
                .Select(c => new
                {
                    c.Name,
                    MostRecentBooks = c.CategoryBooks.Select(c => new
                    {
                        BookName = c.Book.Title,
                        ReleaseDate = c.Book.ReleaseDate
                    })
                    .OrderByDescending(a => a.ReleaseDate)
                    .Take(3)
                })
                .OrderBy(c => c.Name)
                .ToArray();

            foreach (var c in categoryRecentBooks)
            {

                output.AppendLine($"--{c.Name}");
                foreach (var b in c.MostRecentBooks)
                {
                    output.AppendLine($"{b.BookName} ({b.ReleaseDate.Value.Year})");
                }
            }

            return output.ToString().TrimEnd();

        }

        public static void IncreasePrices(BookShopContext context)
        {
            //context.Books
            //         .Where(b => b.ReleaseDate.Value.Year < 2010)
            //         .ToList()
            //         .ForEach(b => b.Price += 5);

           
            //Faster way to do 
            IQueryable<Book> booksBeforeYear = context.Books
                   .Where(b => b.ReleaseDate.Value.Year < 2010);

            foreach (var b in booksBeforeYear)
            {
                b.Price += 5;
            }

            context.SaveChanges();
        
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var booksToDelete = context.Books
                .Where(b => b.Copies < 4200)
                .ToArray();

            context.Books.RemoveRange(booksToDelete);
               
            context.SaveChanges();


            return booksToDelete.Count();
        }
    }
}
