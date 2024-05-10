using System.ComponentModel.DataAnnotations;

namespace Kolokwium_s27662.Models.DTOs;

public class AuthorDTO
{
    [MaxLength(50)]
    public string FirstName { get; set; }
    [MaxLength(100)]
    public string LastName { get; set; }
}