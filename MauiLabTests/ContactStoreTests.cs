using MauiLab.Contacts;
using Contact = MauiLab.Contacts.Contact;

namespace MauiLabTests;

public class ContactStoreTests
{
    [Fact]
    public async Task GenerateFillsTheNestedCounts()
    {
        using var dir = new TempDir();
        var store = new ContactStore(dir.Path, new CountingCatalog());

        await store.GenerateAsync();
        var contacts = store.Load();

        Assert.Equal(1000, contacts.Count);
        Assert.Equal(10, contacts.Count(contact => contact.Group == ContactGroup.Friend));
        Assert.Equal(100, contacts.Count(contact => contact.Group is ContactGroup.Friend or ContactGroup.Family));
        Assert.True(store.CanReset);
        Assert.True(File.Exists(Path.Combine(dir.Path, "portraits", "shared.jpg")));
    }

    [Fact]
    public async Task AFailedGenerateLeavesNoDatabase()
    {
        using var dir = new TempDir();
        var store = new ContactStore(dir.Path, new CountingCatalog { Fail = true });

        await Assert.ThrowsAsync<IOException>(() => store.GenerateAsync());

        Assert.False(store.HasDatabase);
        Assert.False(store.CanReset);
        Assert.False(Directory.Exists(dir.Path + ".incoming"));
    }

    [Fact]
    public async Task ResetRestoresTheCachedBookWithoutDownloading()
    {
        using var dir = new TempDir();
        var catalog = new CountingCatalog();
        var store = new ContactStore(dir.Path, catalog);
        await store.GenerateAsync();
        var edited = store.Load()[0];
        edited.City = "Changed";
        using (var db = ContactsContext.Open(store.DatabasePath))
        {
            db.Update(edited);
            db.SaveChanges();
        }

        var downloads = catalog.Downloads;
        store.Reset();

        Assert.Equal(downloads, catalog.Downloads);
        Assert.NotEqual("Changed", store.Load()[0].City);
    }

    [Fact]
    public void ResetIsUnavailableWithoutACache()
    {
        using var dir = new TempDir();
        var store = new ContactStore(dir.Path, new CountingCatalog());

        Assert.False(store.CanReset);
        Assert.Throws<InvalidOperationException>(store.Reset);
    }

    sealed class CountingCatalog : IContactCatalog
    {
        public bool Fail { get; init; }

        public int Downloads { get; private set; }

        public Task<CatalogDownload> DownloadAsync(IProgress<GenerateProgress>? progress, CancellationToken cancellationToken = default)
        {
            Downloads++;
            if (Fail)
                throw new IOException("download failed");

            progress?.Report(new GenerateProgress(0, 1));
            var contacts = Enumerable.Range(0, 1000).Select(index => new Contact
            {
                FirstName = "Ada",
                LastName = $"Person{index:0000}",
                City = index % 2 == 0 ? "Austin" : "Boise",
                State = "TX",
                PostalCode = "73301",
                Phone = "5125550100",
                Email = $"p{index}@example.com",
                Group = ContactImporter.GroupForIndex(index),
                PhotoFile = "portraits/shared.jpg"
            }).ToList();
            progress?.Report(new GenerateProgress(1, 1));
            return Task.FromResult(new CatalogDownload
            {
                Contacts = contacts,
                Files = [new CatalogFile { RelativePath = "portraits/shared.jpg", Bytes = [1, 2, 3] }]
            });
        }
    }

    sealed class TempDir : IDisposable
    {
        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mauilab-contacts-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);
            if (Directory.Exists(Path + ".incoming"))
                Directory.Delete(Path + ".incoming", true);
            if (Directory.Exists(Path + ".previous"))
                Directory.Delete(Path + ".previous", true);
        }
    }
}
