using LibraryManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Infrastructure.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().Property(b => b.isDeleted).HasDefaultValue(false);
            // Global query filter
            modelBuilder.Entity<Book>().HasQueryFilter(b => !b.isDeleted);
            modelBuilder.Entity<Member>().HasQueryFilter(b => !b.isDeleted);

            modelBuilder.Entity<Book>().HasIndex(b => b.ISBN).IsUnique();
            modelBuilder.Entity<ApplicationUser>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(m => m.Email).IsUnique();

            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.Book)
                .WithMany()
                .HasForeignKey(br => br.BookId);

            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.Member)
                .WithMany()
                .HasForeignKey(br => br.MemberId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
