using Microsoft.EntityFrameworkCore;

namespace MauiLab.Contacts;

public sealed class ContactEditSession : IDisposable
{
    readonly string _databasePath;
    readonly ContactsContext? _db;
    readonly bool _isNew;

    ContactEditSession(string databasePath, ContactsContext? db, Contact contact, bool isNew)
    {
        _databasePath = databasePath;
        _db = db;
        Contact = contact;
        _isNew = isNew;
    }

    public Contact Contact { get; }

    public bool IsNew => _isNew;

    public static ContactEditSession Begin(string databasePath, Contact contact)
    {
        var db = ContactsContext.Open(databasePath);
        db.Attach(contact);
        return new ContactEditSession(databasePath, db, contact, isNew: false);
    }

    public static ContactEditSession BeginNew(string databasePath, Contact contact) =>
        new(databasePath, null, contact, isNew: true);

    public bool IsDirty
    {
        get
        {
            if (_isNew)
                return true;

            _db!.ChangeTracker.DetectChanges();
            return _db.Entry(Contact).State == EntityState.Modified;
        }
    }

    public void Revert()
    {
        if (_isNew)
            return;

        var entry = _db!.Entry(Contact);
        entry.CurrentValues.SetValues(entry.OriginalValues);
        entry.State = EntityState.Unchanged;
    }

    public void Save()
    {
        if (_isNew)
        {
            using var db = ContactsContext.Open(_databasePath);
            db.Contacts.Add(Contact);
            db.SaveChanges();
            return;
        }

        _db!.SaveChanges();
    }

    public void Delete()
    {
        if (_isNew)
            return;

        _db!.Remove(Contact);
        _db.SaveChanges();
    }

    public void Dispose() => _db?.Dispose();
}
