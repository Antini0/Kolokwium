using Kolokwium.Models;
using Microsoft.Data.SqlClient;

namespace Kolokwium.Repositories;

public class BooksRepository : IBooksRepository 
{
    private readonly IConfiguration _configuration;
    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Book> GetBook(int pk)
    {
        var query = @"SELECT 
                            books.PK as id,
                            books.Title as title,
                            authors.PK as author_id,
                            authors.first_name as firstname,
                            authors.last_name as lastname
                       FROM books
                       JOIN books_authors on books_authors.FK_book = books.PK
                       JOIN authors on authors.PK = books_authors.FK_author
                       WHERE books.PK = @PK";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@PK", pk);
	    
        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();
        var booksPKOrdinal = reader.GetOrdinal("id");
        var authorsPKOrdinal = reader.GetOrdinal("author_id");
        var booksTitleOrdinal = reader.GetOrdinal("title");
        var authorsFirstNameOrdinal = reader.GetOrdinal("firstname");
        var authorsLastNameOrdinal = reader.GetOrdinal("lastname");

        Book book = null;
        while (await reader.ReadAsync())
        {
            if (book is not null)
            {
                book.Authors.Add(new Author()
                {
                    PK = reader.GetInt32(authorsPKOrdinal),
                    First_na = reader.GetString(authorsFirstNameOrdinal),
                    Last_na = reader.GetString(authorsLastNameOrdinal)
                });
            }
            else
            {
                book = new Book()
                {
                    PK = reader.GetInt32(booksPKOrdinal),
                    Title = reader.GetString(booksTitleOrdinal),
                    Authors = new List<Author>()
                    {
                        new Author()
                        {
                            PK = reader.GetInt32(authorsPKOrdinal),
                            First_na = reader.GetString(authorsFirstNameOrdinal),
                            Last_na = reader.GetString(authorsLastNameOrdinal)  
                        }
                    }
                };

            }
        }

        if (book is null) throw new Exception();
        
        return book;
    }

    public async Task AddBookWithAuthors(Book book)
    {
        var insert = @"INSERT INTO books VALUES(@Title);
                        SELECT @@IDENTITY as PK;";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
	    
        command.Connection = connection;
        command.CommandText = insert;
        
        command.Parameters.AddWithValue("@Title", book.Title);
        
        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        
        try
        {
            var id = await command.ExecuteScalarAsync();
    
            foreach (var author in book.Authors)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO books_authors VALUES(@FK_book, @FK_author)";
                command.Parameters.AddWithValue("@FK_book", book.PK);
                command.Parameters.AddWithValue("@FK_author", author.PK);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}