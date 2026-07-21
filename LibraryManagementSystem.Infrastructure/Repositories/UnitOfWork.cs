using LibraryManagementSystem.Core.Entities;
using LibraryManagementSystem.Core.Interfaces;
using LibraryManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryDbContext _context;
        public IGenericRepository<Book> Books { get; }
        public IGenericRepository<Member> Members { get; }
        public IBorrowRecordRepository BorrowRecords { get; }
        public IGenericRepository<ApplicationUser> Users { get; }

        public UnitOfWork(LibraryDbContext context)
        {
            _context = context;
            Books = new GenericRepository<Book>(_context);
            Members = new GenericRepository<Member>(_context);
            BorrowRecords = new BorrowRecordRepository(_context);
            Users = new GenericRepository<ApplicationUser>(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
