using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CommaxIoTApp.Models
{
    public class CCTVDbContext : DbContext
    {
        public DbSet<CCTV> CCTV { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=cctv.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // CCTVId required.
            modelBuilder.Entity<CCTV>()
                .Property(c => c.CCTVId)
                .IsRequired();

            modelBuilder.Entity<CCTV>()
                .Property(c => c.IpAddress)
                .IsRequired();

            modelBuilder.Entity<CCTV>()
                .Property(c => c.CCTVName)
                .IsRequired();

        }
    }
}
