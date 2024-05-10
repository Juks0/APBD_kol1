namespace Kolokwium_s27662.Models.DTOs;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<AuthorDTO> Authors { get; set; }
}

