using Kolokwium.Models;

namespace Kolokwium.Repositories;

public interface IBooksRepository
{
    Task<Book> GetBook(int pk);
    Task AddBookWithAuthors(Book book);
}