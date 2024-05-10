using Kolokwium_s27662.Models.DTOs;



    public interface IBookRepository
    {
        Task<Book> GetBookAuthors(int id);
        Task<Book> AddNewBookWithAuthors(BookDTO newBook);
        public Task<bool> DoesAuthorExist(string name, string lastname);
    }

  
