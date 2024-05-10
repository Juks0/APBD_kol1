using Kolokwium_s27662.Models.DTOs;

namespace Kolokwium_s27662.Controllerss;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet("{id}/authors")]
        public async Task<IActionResult> GetBookAuthors(int id)
        {
            try
            {
                var bookAuthors = await _bookRepository.GetBookAuthors(id);
                if (bookAuthors == null)
                {
                    return NotFound($"Book with ID {id} not found");
                }

                return Ok(bookAuthors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewBookWithAuthors(BookDTO newBook)
        {
            var bookAuthors = await _bookRepository.DoesAuthorExist(newBook.Authors[0].FirstName,newBook.Authors[0].LastName);
            if (bookAuthors == null)
            {
                return NotFound($"Author not found");
            }
            
            try
            {
                var book = await _bookRepository.AddNewBookWithAuthors(newBook);
                return CreatedAtAction(nameof(GetBookAuthors), new { id = book.Id }, book);
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
