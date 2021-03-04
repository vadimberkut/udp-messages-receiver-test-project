using MessageReceiver.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageReceiver.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SenderEntity> Senders { get; set; }
        //public DbSet<MessageEntity> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // NB: in SQLite all string types are mapped to TEXT type
            // https://www.sqlite.org/datatype3.html

            modelBuilder.Entity<SenderEntity>()
                .ToTable("Senders")
                .HasKey(x => x.Id);
            modelBuilder.Entity<SenderEntity>()
                .Property(x => x.Id)
                .HasColumnType("nvarchar(50)");

            //modelBuilder.Entity<MessageEntity>().ToTable("Messages");
        }
    }
}
