using Microsoft.EntityFrameworkCore;

namespace MauiLab.Contacts;

public sealed class ContactsContext : DbContext
{
    public ContactsContext(DbContextOptions<ContactsContext> options)
        : base(options)
    {
    }

    public DbSet<Contact> Contacts => Set<Contact>();

    public static ContactsContext Open(string databasePath)
    {
        var options = new DbContextOptionsBuilder<ContactsContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;
        return new ContactsContext(options);
    }
}
