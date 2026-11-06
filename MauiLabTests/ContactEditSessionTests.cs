using MauiLab.Contacts;
using Microsoft.EntityFrameworkCore;
using Contact = MauiLab.Contacts.Contact;

namespace MauiLabTests;

public class ContactEditSessionTests
{
    [Fact]
    public void RevertRestoresTheOriginalValues()
    {
        using var dir = new StoreDir();
        var contact = dir.LoadAda();
        using var session = ContactEditSession.Begin(dir.DatabasePath, contact);
        contact.City = "Changed";

        Assert.True(session.IsDirty);
        session.Revert();

        Assert.Equal("Billings", contact.City);
        Assert.False(session.IsDirty);
    }

    [Fact]
    public void SaveWritesTheEdit()
    {
        using var dir = new StoreDir();
        var contact = dir.LoadAda();
        using (var session = ContactEditSession.Begin(dir.DatabasePath, contact))
        {
            contact.City = "London";
            session.Save();
        }

        Assert.Equal("London", dir.LoadAda().City);
    }

    [Fact]
    public void DroppingANewCardInsertsNothing()
    {
        using var dir = new StoreDir();
        var blank = new Contact { FirstName = "New", LastName = "Person" };
        using (var session = ContactEditSession.BeginNew(dir.DatabasePath, blank))
        {
            Assert.True(session.IsDirty);
            session.Delete();
        }

        Assert.Equal(1, dir.Count());
    }

    [Fact]
    public void DeleteRemovesTheRowAndLeavesThePortrait()
    {
        using var dir = new StoreDir();
        var portrait = Path.Combine(dir.Path, "portraits", "shared.jpg");
        var contact = dir.LoadAda();
        using (var session = ContactEditSession.Begin(dir.DatabasePath, contact))
            session.Delete();

        Assert.Equal(0, dir.Count());
        Assert.True(File.Exists(portrait));
    }

    sealed class StoreDir : IDisposable
    {
        public StoreDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mauilab-edit-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(System.IO.Path.Combine(Path, "portraits"));
            File.WriteAllBytes(System.IO.Path.Combine(Path, "portraits", "shared.jpg"), [1]);
            using var db = ContactsContext.Open(DatabasePath);
            db.Database.EnsureCreated();
            db.Contacts.Add(new Contact
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                City = "Billings",
                State = "MI",
                PostalCode = "63104",
                Phone = "2727900888",
                Email = "ada@example.com",
                PhotoFile = "portraits/shared.jpg"
            });
            db.SaveChanges();
        }

        public string Path { get; }

        public string DatabasePath => System.IO.Path.Combine(Path, "contacts.db");

        public Contact LoadAda()
        {
            using var db = ContactsContext.Open(DatabasePath);
            return db.Contacts.AsNoTracking().Single();
        }

        public int Count()
        {
            using var db = ContactsContext.Open(DatabasePath);
            return db.Contacts.Count();
        }

        public void Dispose()
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);
        }
    }
}
