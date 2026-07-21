using LibraryManagementSystem.Core.Entities;
using LibraryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public BooksController(IUnitOfWork uow) => _uow = uow;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _uow.Books.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _uow.Books.GetByIdAsync(id);
            return book == null ? NotFound() : Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Book book)
        {
            await _uow.Books.AddAsync(book);
            await _uow.CompleteAsync();
            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Book updated)
        {
            var book = await _uow.Books.GetByIdAsync(id);
            if (book == null) return NotFound();

            book.Title = updated.Title;
            book.Author = updated.Author;
            book.ISBN = updated.ISBN;
            book.TotalCopies = updated.TotalCopies;
            book.AvailableCopies = updated.AvailableCopies;

            _uow.Books.Update(book);
            await _uow.CompleteAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _uow.Books.GetByIdAsync(id);
            if (book == null) return NotFound();

            book.isDeleted = true; 
            _uow.Books.Update(book);
            await _uow.CompleteAsync();
            return NoContent();
        }
    }
}
