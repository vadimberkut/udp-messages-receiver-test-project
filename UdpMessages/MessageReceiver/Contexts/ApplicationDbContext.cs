using MessageReceiver.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        public DbSet<MessageEntity> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // NB: in SQLite all string types are mapped to TEXT type
            // Dates are stored as TEXT when ISO8601 strings
            // https://www.sqlite.org/datatype3.html

            // SenderEntity
            modelBuilder.Entity<SenderEntity>()
                .ToTable("Senders")
                .HasKey(x => x.Id);
            modelBuilder.Entity<SenderEntity>()
                .Property(x => x.Id)
                .HasColumnType("nvarchar(50)");

            // MessageEntity
            modelBuilder.Entity<MessageEntity>()
                .ToTable("Messages")
                .HasKey(x => x.Id);
            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.Id)
                .HasColumnType("nvarchar(50)");
            modelBuilder.Entity<MessageEntity>()
                .HasOne(x => x.Sender)
                .WithMany(x => x.Messages);

            // fix DateTimeOffset translation issue for SQLite
            // this ends up in storing DateTimeOffset as INTEGER
            // https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                                                                                || p.PropertyType == typeof(DateTimeOffset?));
                    foreach (var property in properties)
                    {
                        modelBuilder
                            .Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter());
                    }
                }
            }
        }
    }
}
