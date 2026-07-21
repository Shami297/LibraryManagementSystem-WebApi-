using LibraryManagementSystem.Core.Entities;
using LibraryManagementSystem.Core.Interfaces;
using LibraryManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public class BorrowRecordRepository : GenericRepository<BorrowRecord>, IBorrowRecordRepository
    {
        private readonly LibraryDbContext _context;

        public BorrowRecordRepository(LibraryDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BorrowRecord>> GetByMemberIdAsync(int memberId)
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)      // eager-load related Book
                .Include(br => br.Member)    // eager-load related Member
                .Where(br => br.MemberId == memberId)
                .OrderByDescending(br => br.BorrowedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowRecord>> GetByBookIdAsync(int bookId)
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.Member)
                .Where(br => br.BookId == bookId)
                .OrderByDescending(br => br.BorrowedAt)
                .ToListAsync();
        }
    }
}