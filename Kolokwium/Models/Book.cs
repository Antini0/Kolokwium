namespace Kolokwium.Models;

public class Book
{
    public int PK { get; set; }
    public string Title { get; set; }
    public List<Author> Authors { get; set; } = null;
}