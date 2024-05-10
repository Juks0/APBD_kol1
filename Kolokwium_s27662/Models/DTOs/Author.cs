using System.ComponentModel.DataAnnotations;

namespace Kolokwium_s27662.Models.DTOs;

public class Author
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string FirstName { get; set; }
    [MaxLength(100)]
    public string LastName { get; set; }
}