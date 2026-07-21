using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Core.Entities;
using LibraryManagementSystem.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public BorrowRecordsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET /api/borrowrecords
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _uow.BorrowRecords.GetAllAsync();
            return Ok(records);
        }

        // POST /api/borrowrecords/borrow
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook(BorrowBookDto dto)
        {
            var book = await _uow.Books.GetByIdAsync(dto.BookId);
            if (book == null)
                return NotFound("Book not found.");

            var member = await _uow.Members.GetByIdAsync(dto.MemberId);
            if (member == null)
                return NotFound("Member not found.");

            if (book.AvailableCopies <= 0)
                throw new InvalidOperationException("No available copies of this book.");

            // Business rule: decrement available copies
            book.AvailableCopies -= 1;
            _uow.Books.Update(book);

            var record = new BorrowRecord
            {
                BookId = book.Id,
                MemberId = member.Id,
                BorrowedAt = DateTime.UtcNow,
                ReturnedAt = null
            };

            await _uow.BorrowRecords.AddAsync(record);

            // Both changes commit together in ONE transaction
            await _uow.CompleteAsync();

            return Ok(record);
        }

        // PUT /api/borrowrecords/return/5
        [HttpPut("return/{id}")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var record = await _uow.BorrowRecords.GetByIdAsync(id);
            if (record == null)
                return NotFound("Borrow record not found.");

            if (record.ReturnedAt != null)
                return BadRequest("This book has already been returned.");

            var book = await _uow.Books.GetByIdAsync(record.BookId);
            if (book == null)
                return NotFound("Associated book not found.");

            // Business rule: mark returned + increment available copies
            record.ReturnedAt = DateTime.UtcNow;
            book.AvailableCopies += 1;

            _uow.BorrowRecords.Update(record);
            _uow.Books.Update(book);

            await _uow.CompleteAsync();

            return Ok(record);
        }

        // GET /api/borrowrecords/member/5
        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetByMemberId(int memberId)
        {
            var member = await _uow.Members.GetByIdAsync(memberId);
            if (member == null)
                return NotFound("Member not found.");

            var records = await _uow.BorrowRecords.GetByMemberIdAsync(memberId);

            var result = records.Select(r => new
            {
                r.Id,
                BookTitle = r.Book.Title,
                BookAuthor = r.Book.Author,
                MemberName = r.Member.FullName,
                r.BorrowedAt,
                r.ReturnedAt,
                IsReturned = r.ReturnedAt != null
            });

            return Ok(result);
        }

        // GET /api/borrowrecords/book/3
        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetByBookId(int bookId)
        {
            var book = await _uow.Books.GetByIdAsync(bookId);
            if (book == null)
                return NotFound("Book not found.");

            var records = await _uow.BorrowRecords.GetByBookIdAsync(bookId);

            var result = records.Select(r => new
            {
                r.Id,
                MemberName = r.Member.FullName,
                MemberEmail = r.Member.Email,
                r.BorrowedAt,
                r.ReturnedAt,
                IsReturned = r.ReturnedAt != null
            });

            return Ok(result);
        }
    }
}