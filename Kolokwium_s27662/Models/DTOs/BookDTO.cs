using System.ComponentModel.DataAnnotations;

namespace Kolokwium_s27662.Models.DTOs;

public class BookDTO
{
    [MaxLength(100)]
    public string Title { get; set; }
    public List<AuthorDTO> Authors { get; set; }
    
}