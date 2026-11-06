using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiLab.Contacts;

public enum ContactFilter
{
    Friends,
    Family,
    All
}

public enum ContactSort
{
    LastName,
    FirstName,
    City
}

public delegate Task<bool> ContactPrompt(string title, string message, string accept, string cancel);

public partial class ContactsPageModel : ObservableObject
{
    readonly ContactStore _store;
    readonly ContactPrompt? _prompt;
    readonly List<ContactItemModel> _all = [];
    ContactEditSession? _session;

    public ContactsPageModel(ContactStore store, ContactPrompt? prompt = null)
    {
        _store = store;
        _prompt = prompt;
        if (store.HasDatabase)
            Load();
        Publish();
    }

    public ObservableCollection<ContactItemModel> Items { get; } = [];

    public ContactFilter Filter { get; private set; } = ContactFilter.Friends;

    public IReadOnlyList<string> FilterLabels { get; private set; } = ["Friends (0)", "Family (0)", "All (0)"];

    public ContactSort Sort { get; private set; } = ContactSort.LastName;

    public bool CanGenerate => !_store.HasDatabase;

    public bool CanQuery => _store.HasDatabase;

    public bool CanReset => _store.CanReset;

    public bool IsGenerating { get; private set; }

    public bool ShowEmpty => !CanQuery && !IsGenerating;

    public bool HasDirtyEdit => _session?.IsDirty == true;

    public string DirectoryPath => _store.DirectoryPath;

    public int Columns { get; private set; } = 1;

    public double GenerateFraction { get; private set; }

    public string GenerateStatus { get; private set; } = "";

    public ContactItemModel? Editing { get; private set; }

    public ContactItemModel? ScrollTarget { get; private set; }

    public bool ScrollToStart { get; private set; }

    public IReadOnlyList<string> PickerPhotos => _store.PickerPhotos();

    public void ApplyCanvasWidth(double width)
    {
        var columns = ContactColumnFit.Columns(width);
        if (columns == Columns)
            return;

        Columns = columns;
        OnPropertyChanged(nameof(Columns));
    }

    public async Task GenerateAsync()
    {
        if (IsGenerating || !CanGenerate)
            return;

        IsGenerating = true;
        GenerateFraction = 0;
        GenerateStatus = "Downloading contacts";
        NotifyChrome();
        try
        {
            var progress = new Progress<GenerateProgress>(value =>
            {
                GenerateFraction = value.Fraction;
                GenerateStatus = value.Total == 0 ? "Downloading contacts" : $"{value.Finished} of {value.Total}";
                OnPropertyChanged(nameof(GenerateFraction));
                OnPropertyChanged(nameof(GenerateStatus));
            });
            await _store.GenerateAsync(progress);
            Filter = ContactFilter.Friends;
            Sort = ContactSort.LastName;
            Load();
            Publish();
            OnPropertyChanged(nameof(Filter));
            OnPropertyChanged(nameof(Sort));
        }
        finally
        {
            IsGenerating = false;
            NotifyChrome();
        }
    }

    public async Task<bool> SetFilterAsync(ContactFilter filter)
    {
        if (filter == Filter)
            return true;
        if (!await YieldEditingAsync())
            return false;

        Filter = filter;
        OnPropertyChanged(nameof(Filter));
        Publish();
        return true;
    }

    public async Task<bool> SetSortAsync(ContactSort sort)
    {
        if (sort == Sort)
            return true;
        if (!await YieldEditingAsync())
            return false;

        Sort = sort;
        OnPropertyChanged(nameof(Sort));
        Publish();
        return true;
    }

    public async Task<bool> NewContactAsync()
    {
        if (!CanQuery || !await YieldEditingAsync())
            return false;

        var contact = new Contact { Group = GroupFor(Filter) };
        var item = new ContactItemModel(contact, _store.DirectoryPath);
        _all.Add(item);
        Items.Insert(0, item);
        RefreshFilterLabels();
        Open(item, ContactEditSession.BeginNew(_store.DatabasePath, contact));
        ScrollToStart = true;
        ScrollTarget = item;
        OnPropertyChanged(nameof(ScrollTarget));
        return true;
    }

    public async Task<bool> EditAsync(ContactItemModel item)
    {
        if (Editing == item)
            return true;
        if (!await YieldEditingAsync())
            return false;

        Open(item, ContactEditSession.Begin(_store.DatabasePath, item.Contact));
        return true;
    }

    public void Save()
    {
        if (_session == null || Editing == null || !Editing.CanSave)
            return;

        var saved = Editing;
        _session.Save();
        Close(revert: false);
        Publish();
        ScrollToStart = false;
        ScrollTarget = saved;
        OnPropertyChanged(nameof(ScrollTarget));
    }

    public void Cancel()
    {
        if (Editing == null)
            return;

        Close(revert: true);
    }

    public async Task DeleteAsync()
    {
        if (_session == null || Editing == null)
            return;

        if (!_session.IsNew)
        {
            var confirmed = await Ask("Delete this contact?", "Delete this contact from the address book?", "Delete", "Cancel");
            if (!confirmed)
                return;

            _session.Delete();
            var item = Editing;
            Close(revert: false);
            _all.Remove(item);
            Items.Remove(item);
            RefreshFilterLabels();
            return;
        }

        Close(revert: true);
    }

    public async Task<string?> LeaveAsync(string destination)
    {
        if (!await YieldEditingAsync())
            return null;

        return destination;
    }

    public async Task ResetAsync()
    {
        if (!CanReset)
            return;

        var confirmed = await Ask(
            "Reset Database?",
            "Replace the contacts with the generated book. Unsaved edits will be lost.",
            "Reset",
            "Cancel");
        if (!confirmed)
            return;

        _session?.Dispose();
        _session = null;
        if (Editing != null)
            Editing.IsEditing = false;
        Editing = null;
        OnPropertyChanged(nameof(Editing));
        _store.Reset();
        Load();
        Publish();
    }

    void Open(ContactItemModel item, ContactEditSession session)
    {
        _session = session;
        Editing = item;
        item.IsEditing = true;
        OnPropertyChanged(nameof(Editing));
    }

    void Close(bool revert)
    {
        var item = Editing;
        var session = _session;
        if (item == null || session == null)
            return;

        if (revert && session.IsNew)
        {
            _all.Remove(item);
            Items.Remove(item);
            RefreshFilterLabels();
        }
        else if (revert)
        {
            session.Revert();
            item.Refresh();
        }

        item.IsEditing = false;
        session.Dispose();
        _session = null;
        Editing = null;
        OnPropertyChanged(nameof(Editing));
    }

    async Task<bool> YieldEditingAsync()
    {
        if (Editing == null || _session == null)
            return true;
        if (_session.IsDirty)
        {
            var discard = await Ask("Discard changes?", "Discard unsaved changes to this contact?", "Discard", "Keep editing");
            if (!discard)
                return false;
        }

        Close(revert: true);
        return true;
    }

    void Load()
    {
        _all.Clear();
        _all.AddRange(_store.Load().Select(contact => new ContactItemModel(contact, _store.DirectoryPath)));
    }

    void Publish()
    {
        var next = _all.Where(Matches).Select((item, index) => (item, index)).OrderBy(row => row, SortOrder()).Select(row => row.item).ToList();
        for (var i = Items.Count - 1; i >= 0; i--)
        {
            if (!next.Contains(Items[i]))
                Items.RemoveAt(i);
        }

        for (var i = 0; i < next.Count; i++)
        {
            var item = next[i];
            var current = Items.IndexOf(item);
            if (current < 0)
                Items.Insert(i, item);
            else if (current != i)
                Items.Move(current, i);
        }

        RefreshFilterLabels();
    }

    void RefreshFilterLabels()
    {
        var friends = _all.Count(item => item.Contact.Group == ContactGroup.Friend);
        var family = _all.Count(item => item.Contact.Group is ContactGroup.Friend or ContactGroup.Family);
        string[] labels =
        [
            $"Friends ({Count(friends)})",
            $"Family ({Count(family)})",
            $"All ({Count(_all.Count)})"
        ];
        if (FilterLabels.SequenceEqual(labels))
            return;

        FilterLabels = labels;
        OnPropertyChanged(nameof(FilterLabels));
    }

    static string Count(int value) => value.ToString("N0", CultureInfo.InvariantCulture);

    void NotifyChrome()
    {
        OnPropertyChanged(nameof(IsGenerating));
        OnPropertyChanged(nameof(ShowEmpty));
        OnPropertyChanged(nameof(CanGenerate));
        OnPropertyChanged(nameof(CanQuery));
        OnPropertyChanged(nameof(CanReset));
        OnPropertyChanged(nameof(GenerateFraction));
        OnPropertyChanged(nameof(GenerateStatus));
    }

    bool Matches(ContactItemModel item) => Filter switch
    {
        ContactFilter.Friends => item.Contact.Group == ContactGroup.Friend,
        ContactFilter.Family => item.Contact.Group is ContactGroup.Friend or ContactGroup.Family,
        _ => true
    };

    IComparer<(ContactItemModel Item, int Index)> SortOrder() => Comparer<(ContactItemModel Item, int Index)>.Create((left, right) =>
    {
        var compare = Sort switch
        {
            ContactSort.FirstName => string.Compare(left.Item.Contact.FirstName, right.Item.Contact.FirstName, StringComparison.OrdinalIgnoreCase),
            ContactSort.City => string.Compare(left.Item.Contact.City, right.Item.Contact.City, StringComparison.OrdinalIgnoreCase),
            _ => string.Compare(left.Item.Contact.LastName, right.Item.Contact.LastName, StringComparison.OrdinalIgnoreCase)
        };
        return compare != 0 ? compare : left.Index.CompareTo(right.Index);
    });

    static ContactGroup GroupFor(ContactFilter filter) => filter switch
    {
        ContactFilter.Friends => ContactGroup.Friend,
        ContactFilter.Family => ContactGroup.Family,
        _ => ContactGroup.Other
    };

    Task<bool> Ask(string title, string message, string accept, string cancel) =>
        _prompt?.Invoke(title, message, accept, cancel) ?? Task.FromResult(false);
}
