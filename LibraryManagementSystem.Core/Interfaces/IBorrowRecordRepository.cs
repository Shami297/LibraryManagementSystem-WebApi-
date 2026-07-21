using LibraryManagementSystem.Core.Entities;

namespace LibraryManagementSystem.Core.Interfaces
{
    public interface IBorrowRecordRepository : IGenericRepository<BorrowRecord>
    {
        Task<IEnumerable<BorrowRecord>> GetByMemberIdAsync(int memberId);
        Task<IEnumerable<BorrowRecord>> GetByBookIdAsync(int bookId);
    }
}