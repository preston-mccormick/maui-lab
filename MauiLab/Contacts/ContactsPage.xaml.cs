namespace MauiLab.Contacts;

public partial class ContactsPage : ContentPage
{
    bool _syncing;
    bool _resume;
    bool _loaded;
    bool _chrome = true;
    bool _committingFilter;
    bool _committingSort;
    int _filterWave;
    int _sortWave;
    IDispatcherTimer? _fitTimer;

    public ContactsPage()
    {
        InitializeComponent();
        ItemsView.SizeChanged += (_, _) => Fit();
    }

    public void SetChromeEnabled(bool enabled)
    {
        _chrome = enabled;
        ToolBar.IsEnabled = enabled;
        SyncTitleReset();
    }

    public void Reveal(ContactItemModel item)
    {
        if (BindingContext is not ContactsPageModel model || !model.Items.Contains(item))
            return;

        Dispatcher.Dispatch(() =>
            Dispatcher.Dispatch(() =>
                ItemsView.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: false)));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Shell.Current != null)
            Shell.Current.Navigating += OnNavigating;

        if (_loaded)
            return;

        _loaded = true;
        var directory = Path.Combine(FileSystem.AppDataDirectory, "contacts");
        var model = new ContactsPageModel(new ContactStore(directory, new RandomUserCatalog()), Ask);
        BindingContext = model;
        _syncing = true;
        SortPicker.ItemsSource = new[] { "Last name", "First name", "City" };
        FilterPicker.ItemsSource = model.FilterLabels.ToList();
        FilterPicker.SelectedIndex = (int)model.Filter;
        SortPicker.SelectedIndex = (int)model.Sort;
        _syncing = false;
        ItemsView.ItemsSource = model.Items;
        model.PropertyChanged += OnModelPropertyChanged;
        SyncTitleReset();
        ApplyLayout();
        Fit();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (Shell.Current != null)
            Shell.Current.Navigating -= OnNavigating;

        if (BindingContext is ContactsPageModel model && model.Editing != null && !model.HasDirtyEdit)
            model.Cancel();
    }

    void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        if (_resume || BindingContext is not ContactsPageModel model || !model.HasDirtyEdit)
            return;
        if (Shell.Current?.CurrentPage != this)
            return;

        var target = e.Target.Location.OriginalString;
        e.Cancel();
        Dispatcher.Dispatch(async () =>
        {
            var next = await model.LeaveAsync(target);
            if (next == null || Shell.Current == null)
                return;

            _resume = true;
            try
            {
                await Shell.Current.GoToAsync(next);
            }
            finally
            {
                _resume = false;
            }
        });
    }

    void OnModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (BindingContext is not ContactsPageModel model)
            return;

        if (e.PropertyName == nameof(ContactsPageModel.Columns))
            ApplyLayout();
        else if (e.PropertyName == nameof(ContactsPageModel.FilterLabels))
            ShowFilters(model);
        else if (e.PropertyName == nameof(ContactsPageModel.Filter))
            SyncPicker(FilterPicker, (int)model.Filter);
        else if (e.PropertyName == nameof(ContactsPageModel.Sort))
            SyncPicker(SortPicker, (int)model.Sort);
        else if (e.PropertyName == nameof(ContactsPageModel.ScrollTarget) && model.ScrollTarget != null)
        {
            var item = model.ScrollTarget;
            var position = model.ScrollToStart ? ScrollToPosition.Start : ScrollToPosition.MakeVisible;
            Dispatcher.Dispatch(() => ItemsView.ScrollTo(item, position: position, animate: true));
        }
        else if (e.PropertyName == nameof(ContactsPageModel.CanReset))
            SyncTitleReset();
    }

    void SyncTitleReset() =>
        TitleReset.IsEnabled = _chrome && BindingContext is ContactsPageModel { CanReset: true };

    void OnFilterChanged(object? sender, EventArgs e)
    {
        if (_syncing || FilterPicker.SelectedIndex < 0)
            return;
        if (FilterPicker.IsFocused)
            Dispatcher.Dispatch(() => FilterPicker.Unfocus());
        else
            ScheduleFilter();
    }

    void OnFilterClosed(object? sender, FocusEventArgs e) => ScheduleFilter();

    void ScheduleFilter()
    {
        var wave = ++_filterWave;
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(350), () =>
        {
            if (wave == _filterWave)
                _ = CommitFilter();
        });
    }

    async Task CommitFilter()
    {
        if (_committingFilter || _syncing || BindingContext is not ContactsPageModel model || FilterPicker.SelectedIndex < 0)
            return;

        var filter = (ContactFilter)FilterPicker.SelectedIndex;
        if (filter == model.Filter)
            return;

        _committingFilter = true;
        try
        {
            if (!await model.SetFilterAsync(filter))
                SyncPicker(FilterPicker, (int)model.Filter);
        }
        finally
        {
            _committingFilter = false;
        }
    }

    void OnSortChanged(object? sender, EventArgs e)
    {
        if (_syncing || SortPicker.SelectedIndex < 0)
            return;
        if (SortPicker.IsFocused)
            Dispatcher.Dispatch(() => SortPicker.Unfocus());
        else
            ScheduleSort();
    }

    void OnSortClosed(object? sender, FocusEventArgs e) => ScheduleSort();

    void ScheduleSort()
    {
        var wave = ++_sortWave;
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(350), () =>
        {
            if (wave == _sortWave)
                _ = CommitSort();
        });
    }

    async Task CommitSort()
    {
        if (_committingSort || _syncing || BindingContext is not ContactsPageModel model || SortPicker.SelectedIndex < 0)
            return;

        var sort = (ContactSort)SortPicker.SelectedIndex;
        if (sort == model.Sort)
            return;

        _committingSort = true;
        try
        {
            if (!await model.SetSortAsync(sort))
                SyncPicker(SortPicker, (int)model.Sort);
        }
        finally
        {
            _committingSort = false;
        }
    }

    void ShowFilters(ContactsPageModel model)
    {
        _syncing = true;
        FilterPicker.ItemsSource = model.FilterLabels.ToList();
        FilterPicker.SelectedIndex = (int)model.Filter;
        _syncing = false;
    }

    void SyncPicker(Picker picker, int index)
    {
        if (picker.SelectedIndex == index)
            return;

        _syncing = true;
        picker.SelectedIndex = index;
        _syncing = false;
    }

    async void OnNewContact(object? sender, EventArgs e)
    {
        if (BindingContext is ContactsPageModel model)
            await model.NewContactAsync();
    }

    async void OnGenerate(object? sender, EventArgs e)
    {
        if (BindingContext is not ContactsPageModel model)
            return;

        try
        {
            await model.GenerateAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Generate Database", ex.Message, "OK");
        }
    }

    async void OnReset(object? sender, EventArgs e)
    {
        if (BindingContext is ContactsPageModel model)
            await model.ResetAsync();
    }

    Task<bool> Ask(string title, string message, string accept, string cancel) =>
        DisplayAlertAsync(title, message, accept, cancel);

    void Fit()
    {
        if (BindingContext is not ContactsPageModel || ItemsView.Width <= 1)
            return;

        var timer = _fitTimer ??= Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(50);
        timer.IsRepeating = false;
        timer.Tick -= ApplyFit;
        timer.Tick += ApplyFit;
        timer.Start();
    }

    void ApplyFit(object? sender, EventArgs e)
    {
        if (BindingContext is ContactsPageModel model && ItemsView.Width > 1)
            model.ApplyCanvasWidth(ItemsView.Width);
    }

    void ApplyLayout()
    {
        if (BindingContext is not ContactsPageModel model)
            return;

        ItemsView.ItemsLayout = new GridItemsLayout(Math.Max(1, model.Columns), ItemsLayoutOrientation.Vertical)
        {
            HorizontalItemSpacing = ContactColumnFit.Gap,
            VerticalItemSpacing = ContactColumnFit.Gap
        };
    }
}
