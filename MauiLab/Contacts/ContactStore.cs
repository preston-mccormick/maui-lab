using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MauiLab.Contacts;

public sealed class ContactStore
{
    readonly IContactCatalog _catalog;

    public ContactStore(string directory, IContactCatalog catalog)
    {
        DirectoryPath = directory;
        _catalog = catalog;
        Directory.CreateDirectory(directory);
    }

    public string DirectoryPath { get; }

    public string DatabasePath => Path.Combine(DirectoryPath, "contacts.db");

    public string SeedPath => Path.Combine(DirectoryPath, "seed.json");

    public bool HasDatabase => File.Exists(DatabasePath);

    public bool CanReset => File.Exists(SeedPath);

    public async Task GenerateAsync(IProgress<GenerateProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var download = await _catalog.DownloadAsync(progress, cancellationToken);
        var incoming = DirectoryPath + ".incoming";
        if (Directory.Exists(incoming))
            Directory.Delete(incoming, true);

        Directory.CreateDirectory(incoming);
        try
        {
            foreach (var file in download.Files)
            {
                var path = Path.Combine(incoming, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllBytesAsync(path, file.Bytes, cancellationToken);
            }

            await File.WriteAllTextAsync(Path.Combine(incoming, "seed.json"), JsonSerializer.Serialize(download.Contacts), cancellationToken);
            using (var db = ContactsContext.Open(Path.Combine(incoming, "contacts.db")))
            {
                db.Database.EnsureCreated();
                db.Contacts.AddRange(download.Contacts);
                db.SaveChanges();
            }

            Promote(incoming);
        }
        catch
        {
            if (Directory.Exists(incoming))
                Directory.Delete(incoming, true);
            throw;
        }
    }

    public IReadOnlyList<Contact> Load()
    {
        if (!HasDatabase)
            return [];

        using var db = ContactsContext.Open(DatabasePath);
        return db.Contacts.AsNoTracking().OrderBy(contact => contact.Id).ToList();
    }

    public IReadOnlyList<string> PickerPhotos()
    {
        var picker = Path.Combine(DirectoryPath, "picker");
        if (!Directory.Exists(picker))
            return [];

        return Directory.GetFiles(picker)
            .Select(path => Path.GetRelativePath(DirectoryPath, path).Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
    }

    public void Reset()
    {
        if (!CanReset)
            throw new InvalidOperationException("No cached contact book.");

        var contacts = JsonSerializer.Deserialize<List<Contact>>(File.ReadAllText(SeedPath)) ?? [];
        foreach (var contact in contacts)
            contact.Id = 0;

        DeleteDatabase();
        using var db = ContactsContext.Open(DatabasePath);
        db.Database.EnsureCreated();
        db.Contacts.AddRange(contacts);
        db.SaveChanges();
    }

    void Promote(string incoming)
    {
        var backup = DirectoryPath + ".previous";
        if (Directory.Exists(backup))
            Directory.Delete(backup, true);

        Directory.Move(DirectoryPath, backup);
        try
        {
            Directory.Move(incoming, DirectoryPath);
            Directory.Delete(backup, true);
        }
        catch
        {
            if (!Directory.Exists(DirectoryPath) && Directory.Exists(backup))
                Directory.Move(backup, DirectoryPath);
            throw;
        }
    }

    void DeleteDatabase()
    {
        SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var path = DatabasePath + suffix;
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
