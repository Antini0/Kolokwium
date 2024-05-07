using Kolokwium.Models;
using Kolokwium.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium.Controllers;

[ApiController]
[Route("[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;

    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }

    [HttpGet("{pk}")]
    public async Task<IActionResult> GetBook(int pk)
    {
        var book = await _booksRepository.GetBook(pk);

        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook(Book book)
    {
        await _booksRepository.AddBookWithAuthors(book);
        
        return Created(Request.Path.Value ?? "api/books", book);
    }
}