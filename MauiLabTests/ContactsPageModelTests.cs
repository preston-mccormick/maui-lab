using MauiLab.Contacts;
using Contact = MauiLab.Contacts.Contact;

namespace MauiLabTests;

public class ContactsPageModelTests
{
    [Fact]
    public void AnEmptyBookOffersGenerateOnly()
    {
        using var dir = new TempDir();
        var model = new ContactsPageModel(new ContactStore(dir.Path, new BookCatalog()));

        Assert.True(model.CanGenerate);
        Assert.False(model.CanQuery);
        Assert.False(model.CanReset);
        Assert.Empty(model.Items);
    }

    [Fact]
    public async Task GenerateOpensOnFriendsByLastName()
    {
        var model = await OpenBook();

        Assert.False(model.CanGenerate);
        Assert.True(model.CanQuery);
        Assert.Equal(ContactFilter.Friends, model.Filter);
        Assert.Equal(ContactSort.LastName, model.Sort);
        Assert.Equal(["Lovelace", "Lovelace"], model.Items.Select(item => item.Contact.LastName).ToArray());
        Assert.Equal("Zoe", model.Items[0].Contact.FirstName);
        Assert.Equal("Amy", model.Items[1].Contact.FirstName);
        Assert.Equal(["Friends (2)", "Family (3)", "All (3)"], model.FilterLabels);
    }

    [Fact]
    public async Task TheCurrentSortDoesNotRebuild()
    {
        var model = await OpenBook();
        var changes = 0;
        model.Items.CollectionChanged += (_, _) => changes++;

        await model.SetSortAsync(ContactSort.LastName);

        Assert.Equal(0, changes);
    }

    [Fact]
    public async Task CityAndFirstNameSortAscendingAndKeepTies()
    {
        var model = await OpenBook();
        await model.SetFilterAsync(ContactFilter.All);

        await model.SetSortAsync(ContactSort.City);
        Assert.Equal(["Zoe", "Grace", "Amy"], model.Items.Select(item => item.Contact.FirstName).ToArray());

        await model.SetSortAsync(ContactSort.FirstName);
        Assert.Equal(["Amy", "Grace", "Zoe"], model.Items.Select(item => item.Contact.FirstName).ToArray());
    }

    [Theory]
    [InlineData(390, 1)]
    [InlineData(768, 2)]
    [InlineData(1024, 3)]
    [InlineData(1440, 4)]
    public void ColumnsFollowTheCardWidth(double width, int columns)
    {
        using var dir = new TempDir();
        var model = new ContactsPageModel(new ContactStore(dir.Path, new BookCatalog()));

        model.ApplyCanvasWidth(width);

        Assert.Equal(columns, model.Columns);
        Assert.Equal(columns, ContactColumnFit.Columns(width));
    }

    [Fact]
    public async Task GenerateProgressIsPicturesFinishedOverTheDistinctCount()
    {
        using var dir = new TempDir();
        var model = new ContactsPageModel(new ContactStore(dir.Path, new BookCatalog()));

        await model.GenerateAsync();

        Assert.Equal(0.75, model.GenerateFraction);
        Assert.Equal("3 of 4", model.GenerateStatus);
    }

    [Fact]
    public async Task NewContactSitsAtTheTopAndTakesTheFilterGroup()
    {
        var model = await OpenBook(accept: true);
        await model.SetFilterAsync(ContactFilter.Friends);

        Assert.True(await model.NewContactAsync());

        Assert.Equal("", model.Items[0].Contact.FirstName);
        Assert.True(model.Items[0].IsEditing);
        Assert.Equal(ContactGroup.Friend, model.Items[0].Contact.Group);
        Assert.Equal(model.Items[0], model.Editing);
        Assert.Same(model.Items[0], model.ScrollTarget);
        Assert.True(model.ScrollToStart);

        model.Cancel();
        await model.SetFilterAsync(ContactFilter.Family);
        await model.NewContactAsync();
        Assert.Equal(ContactGroup.Family, model.Editing!.Contact.Group);

        model.Cancel();
        await model.SetFilterAsync(ContactFilter.All);
        await model.NewContactAsync();
        Assert.Equal(ContactGroup.Other, model.Editing!.Contact.Group);
    }

    [Fact]
    public async Task ABlankNewCardAsksBeforeLeaving()
    {
        var prompts = new PromptLog(accept: false);
        var model = await OpenBook(prompts);
        await model.NewContactAsync();

        Assert.Null(await model.LeaveAsync("//Colors"));
        Assert.Equal("Discard changes?", prompts.Titles.Single());
        Assert.NotNull(model.Editing);
    }

    [Fact]
    public async Task SaveSortsTheCardAndNamesTheScrollTarget()
    {
        var model = await OpenBook(accept: true);
        await model.SetFilterAsync(ContactFilter.All);
        await model.NewContactAsync();
        model.Editing!.FirstName = "Nia";
        model.Editing.LastName = "Hopper";

        model.Save();

        Assert.Null(model.Editing);
        Assert.Equal(["Grace", "Nia", "Zoe", "Amy"], model.Items.Select(item => item.Contact.FirstName).ToArray());
        Assert.Equal("Nia", model.ScrollTarget!.Contact.FirstName);
        Assert.False(model.ScrollTarget.IsEditing);
    }

    [Fact]
    public async Task CancelRestoresAnEditAndRemovesANewCard()
    {
        var model = await OpenBook(accept: true);
        var grace = model.Items[0];
        await model.EditAsync(grace);
        grace.City = "Changed";

        model.Cancel();

        Assert.Equal("Austin", grace.Contact.City);
        Assert.Null(model.Editing);

        await model.NewContactAsync();
        model.Cancel();
        Assert.DoesNotContain(model.Items, item => item.Contact.FirstName == "");
    }

    [Fact]
    public async Task DeleteAsksOnceForASavedCard()
    {
        var prompts = new PromptLog(accept: false);
        var model = await OpenBook(prompts);
        var grace = model.Items[0];
        await model.EditAsync(grace);

        await model.DeleteAsync();

        Assert.Equal("Delete this contact?", prompts.Titles.Single());
        Assert.Contains(grace, model.Items);
        Assert.Equal(grace, model.Editing);

        prompts.Accept = true;
        await model.DeleteAsync();
        Assert.DoesNotContain(grace, model.Items);
        Assert.Null(model.Editing);
    }

    [Fact]
    public async Task DeleteOfANewCardDoesNotAsk()
    {
        var prompts = new PromptLog(accept: true);
        var model = await OpenBook(prompts);
        await model.NewContactAsync();
        prompts.Titles.Clear();

        await model.DeleteAsync();

        Assert.Empty(prompts.Titles);
        Assert.Null(model.Editing);
        Assert.Equal(2, model.Items.Count);
    }

    [Fact]
    public async Task KeepEditingCancelsTheNextAction()
    {
        var prompts = new PromptLog(accept: false);
        var model = await OpenBook(prompts);
        var grace = model.Items[0];
        await model.EditAsync(grace);
        grace.City = "Changed";

        Assert.False(await model.SetSortAsync(ContactSort.City));
        Assert.Equal(ContactSort.LastName, model.Sort);
        Assert.Equal(grace, model.Editing);
        Assert.Equal("Changed", grace.Contact.City);
    }

    [Fact]
    public async Task ACleanCardDoesNotAsk()
    {
        var prompts = new PromptLog(accept: false);
        var model = await OpenBook(prompts);
        await model.EditAsync(model.Items[0]);

        Assert.True(await model.SetFilterAsync(ContactFilter.Family));

        Assert.Empty(prompts.Titles);
        Assert.Equal(ContactFilter.Family, model.Filter);
        Assert.Null(model.Editing);
    }

    [Fact]
    public async Task DiscardContinuesToTheCancelledTarget()
    {
        var model = await OpenBook(accept: true);
        await model.NewContactAsync();

        Assert.Equal("//Colors", await model.LeaveAsync("//Colors"));
        Assert.Null(model.Editing);
        Assert.Equal(2, model.Items.Count);
    }

    [Fact]
    public async Task ResetRebuildsADirtyBookFromTheCache()
    {
        var prompts = new PromptLog(accept: true);
        var model = await OpenBook(prompts);
        var grace = model.Items[0];
        await model.EditAsync(grace);
        grace.City = "Changed";
        prompts.Titles.Clear();

        await model.ResetAsync();

        Assert.Equal(["Reset Database?"], prompts.Titles);
        Assert.Equal("Austin", model.Items.Single(item => item.Contact.FirstName == "Zoe").Contact.City);
        Assert.Null(model.Editing);
    }

    static Task<ContactsPageModel> OpenBook() => OpenBook(accept: true);

    static Task<ContactsPageModel> OpenBook(bool accept) =>
        OpenBook(new PromptLog(accept));

    static async Task<ContactsPageModel> OpenBook(PromptLog prompts)
    {
        var dir = new TempDir();
        var model = new ContactsPageModel(new ContactStore(dir.Path, new BookCatalog()), prompts.Ask);
        await model.GenerateAsync();
        return model;
    }

    sealed class PromptLog(bool accept)
    {
        public bool Accept { get; set; } = accept;

        public List<string> Titles { get; } = [];

        public Task<bool> Ask(string title, string message, string acceptText, string cancelText)
        {
            Titles.Add(title);
            return Task.FromResult(Accept);
        }
    }

    sealed class BookCatalog : IContactCatalog
    {
        public Task<CatalogDownload> DownloadAsync(IProgress<GenerateProgress>? progress, CancellationToken cancellationToken = default)
        {
            progress?.Report(new GenerateProgress(0, 1));
            progress?.Report(new GenerateProgress(3, 4));
            return Task.FromResult(new CatalogDownload
            {
                Contacts =
                [
                    Person("Zoe", "Lovelace", "Austin", ContactGroup.Friend),
                    Person("Amy", "Lovelace", "Boise", ContactGroup.Friend),
                    Person("Grace", "Hopper", "Austin", ContactGroup.Family)
                ],
                Files = [new CatalogFile { RelativePath = "portraits/shared.jpg", Bytes = [1] }]
            });
        }

        static Contact Person(string first, string last, string city, ContactGroup group) => new()
        {
            FirstName = first,
            LastName = last,
            City = city,
            State = "TX",
            PostalCode = "73301",
            Phone = "5125550100",
            Email = $"{first.ToLowerInvariant()}@example.com",
            Group = group
        };
    }

    sealed class TempDir : IDisposable
    {
        public TempDir() => Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mauilab-page-" + Guid.NewGuid().ToString("N"));

        public string Path { get; }

        public void Dispose()
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            foreach (var suffix in new[] { "", ".incoming", ".previous" })
            {
                var path = Path + suffix;
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
        }
    }
}
