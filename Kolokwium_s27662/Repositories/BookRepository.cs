using Kolokwium_s27662.Models.DTOs;
namespace Kolokwium_s27662.Repositories;

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;


    public class BookRepository : IBookRepository
    {
        private readonly IConfiguration _configuration;

        public BookRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> DoesBookExist(int id)
        {
            var query = "SELECT 1 FROM books WHERE PK = @ID";

            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            command.CommandText = query;
            command.Parameters.AddWithValue("@ID", id);

            await connection.OpenAsync();

            var res = await command.ExecuteScalarAsync();

            return res is not null;
        }
        public async Task<bool> DoesAuthorExist(string name, string lastname)
        {
            var query = "SELECT 1 FROM authors WHERE FirstName = @FirstName AND LastName = @LastName";
            bool authorExists = false;
    
            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            using SqlCommand command = new SqlCommand(query, connection);
    
            command.Parameters.AddWithValue("@FirstName", name);
            command.Parameters.AddWithValue("@LastName", lastname);
    
            try
            {
                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                authorExists = (result != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return authorExists;
        }


        public async Task<Book> GetBookAuthors(int id)
        {
            bool bookExists = await DoesBookExist(id);
            if (!bookExists)
            {
                throw new Exception($"Book with ID {id} does not exist.");
            }
            
            var query = @"
                SELECT
                    b.PK AS BookId,
                    b.title AS Title,
                    a.first_name AS FirstName,
                    a.last_name AS LastName
                FROM
                    books b
                INNER JOIN
                    books_authors ba ON b.PK = ba.FK_book
                INNER JOIN
                    authors a ON ba.FK_author = a.PK
                WHERE
                    b.PK = @ID";

            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ID", id);

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            Book book = null;
            while (await reader.ReadAsync())
            {
                if (book == null)
                {
                    book = new Book()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("BookId")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Authors = new List<AuthorDTO>()
                    };
                }

                book.Authors.Add(new AuthorDTO()
                {
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName"))
                });
            }

            return book;
        }

public async Task<Book> AddNewBookWithAuthors(BookDTO newBook)
{
    var insertBook = @"
        INSERT INTO books (title)
        VALUES (@Title);
        SELECT SCOPE_IDENTITY() AS BookId;";

    using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
    await connection.OpenAsync();
    var transaction = connection.BeginTransaction();

    try
    {
        using SqlCommand command = new SqlCommand(insertBook, connection, transaction);
        command.Parameters.AddWithValue("@Title", newBook.Title);
        var newBookId = await command.ExecuteScalarAsync();

        if (newBookId == null || newBookId == DBNull.Value)
        {
            transaction.Rollback();
            throw new Exception("Failed to retrieve book ID.");
        }

        foreach (var author in newBook.Authors)
        {
            var authorExistsQuery = @"
                SELECT authors.PK AS authorPK, authors.first_name AS authorFirstName, authors.last_name AS authorLastName 
                FROM authors 
                WHERE first_name = @FirstName AND last_name = @LastName;";

            using SqlCommand authorExistsCommand = new SqlCommand(authorExistsQuery, connection, transaction);
            authorExistsCommand.Parameters.AddWithValue("@FirstName", author.FirstName);
            authorExistsCommand.Parameters.AddWithValue("@LastName", author.LastName);

            var existingAuthorId = await authorExistsCommand.ExecuteScalarAsync();

            if (existingAuthorId == null || existingAuthorId == DBNull.Value)
            {
                transaction.Rollback();
                throw new Exception("Author does not exist.");
            }

            int authorId = Convert.ToInt32(existingAuthorId);

            var insertBookAuthor = @"
                INSERT INTO books_authors (FK_book, FK_author)
                VALUES (@BookId, @AuthorId);";

            using SqlCommand bookAuthorCommand = new SqlCommand(insertBookAuthor, connection, transaction);
            bookAuthorCommand.Parameters.AddWithValue("@BookId", newBookId);
            bookAuthorCommand.Parameters.AddWithValue("@AuthorId", authorId);
            await bookAuthorCommand.ExecuteNonQueryAsync();
        }

        var selectBookQuery = @"
            SELECT books.PK AS bookTest, books.title AS bookTitle, authors.first_name AS authorFirstName, authors.last_name AS authorLastName
            FROM books
            INNER JOIN books_authors ON books.PK = books_authors.FK_book
            INNER JOIN authors ON books_authors.FK_author = authors.PK
            WHERE books.PK = @BookId;";

        using SqlCommand selectBookCommand = new SqlCommand(selectBookQuery, connection, transaction);
        selectBookCommand.Parameters.AddWithValue("@BookId", newBookId);

        using SqlDataReader reader = await selectBookCommand.ExecuteReaderAsync();

        Book book = null;

        while (await reader.ReadAsync())
        {
            if (book == null)
            {
                book = new Book()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("bookTest")),
                    Title = reader.GetString(reader.GetOrdinal("bookTitle")),
                    Authors = new List<AuthorDTO>()
                };
            }

            book.Authors.Add(new AuthorDTO()
            {
                FirstName = reader.GetString(reader.GetOrdinal("authorFirstName")),
                LastName = reader.GetString(reader.GetOrdinal("authorLastName"))
            });
        }
  //      await transaction.CommitAsync();
        return book;
    }
    catch (Exception)
    {
        transaction.Rollback();
        throw;
    }
}

    }
