using LibraryManagementSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Book> Books { get; }
        IGenericRepository<Member> Members { get; }
        IBorrowRecordRepository BorrowRecords { get; }
        IGenericRepository<ApplicationUser> Users { get; }
        Task<int> CompleteAsync();
    }
}
