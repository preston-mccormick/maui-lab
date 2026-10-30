using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiLab.Colors
{
    public partial class ColorsPageModel : ObservableObject
    {
        private const string FlexLayoutControl = "Flex layout";
        private const string CollectionViewControl = "Collection view";
        private const int FlexBatchSize = 40;
        private static readonly TimeSpan PickerSettle = TimeSpan.FromMilliseconds(350);

        private readonly ObservableCollection<ColorModel> _flexItems = new();
        private ColorModel? _selectedBackgroundColor;
        private ColorSortOption _selectedSortOption;
        private bool _ready;
        private bool _draining;
        private bool _listDirty;
        private bool _sortDirty;
        private bool _controlDirty;
        private bool _fontDirty;

        public RelayCommand<ColorModel> SwatchTappedCommand => new(SwatchTapped);

        public ColorsPageModel()
        {
            ColorLists = ColorCatalog.Load();
            SelectedColorList = ColorLists[0];
            Swatches = new ObservableCollection<ColorModel>(SelectedColorList.Colors);
            Title = "Named Colors";
            SortOptions = ColorSortOption.GetColorSortOptions();
            _selectedSortOption = SortOptions.First();
            SwatchesBackground = GetThemeBackground();
            if (Application.Current is Application app)
                app.RequestedThemeChanged += OnRequestedThemeChanged;
            _ready = true;
        }

        public ColorModel? SelectedBackgroundColor
        {
            get => _selectedBackgroundColor;
            set => SetProperty(ref _selectedBackgroundColor, value);
        }

        public ColorSortOption SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (SetProperty(ref _selectedSortOption, value))
                    Mark(sort: true);
            }
        }

        public IReadOnlyList<string> ControlOptions { get; } = [FlexLayoutControl, CollectionViewControl];

        [ObservableProperty]
        public partial string SelectedControl { get; set; } = CollectionViewControl;

        public IReadOnlyList<ColorListOption> ColorLists { get; }

        [ObservableProperty]
        public partial ColorListOption SelectedColorList { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VirtualSwatches))]
        public partial ObservableCollection<ColorModel> Swatches { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UseFlexLayout))]
        [NotifyPropertyChangedFor(nameof(VirtualSwatches))]
        public partial bool UseCollectionView { get; set; } = true;

        public bool UseFlexLayout => !UseCollectionView;

        public ObservableCollection<ColorModel> FlexSwatches => _flexItems;

        public ObservableCollection<ColorModel>? VirtualSwatches => UseCollectionView ? Swatches : null;

        [ObservableProperty]
        public partial List<ColorSortOption> SortOptions { get; set; }

        [ObservableProperty]
        public partial bool ShowTimings { get; set; } = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasTimingNotice))]
        public partial string TimingText { get; set; } = "";

        public bool HasTimingNotice => TimingText.Length > 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontSizeText))]
        public partial double FontSizePreview { get; set; } = 14;

        [ObservableProperty]
        public partial double SwatchFontSize { get; set; } = 14;

        public string FontSizeText => FontSizePreview.ToString("0", CultureInfo.InvariantCulture);

        [ObservableProperty]
        public partial Color SwatchesBackground { get; set; }

        public string Title { get; set; }

        public void CommitFontSize()
        {
            if (!_ready || Math.Abs(SwatchFontSize - FontSizePreview) < 0.05)
                return;

            Mark(font: true);
        }

        [RelayCommand]
        private void DismissTiming() => TimingText = "";

        partial void OnSelectedControlChanged(string value) => Mark(control: true);

        partial void OnSelectedColorListChanged(ColorListOption value) => Mark(list: true);

        partial void OnShowTimingsChanged(bool value)
        {
            if (!value)
                TimingText = "";
        }

        private void SwatchTapped(ColorModel? model)
        {
            if (model == null)
                return;

            SelectedBackgroundColor = model;
            SwatchesBackground = model.Color;
        }

        private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            if (SelectedBackgroundColor == null)
                SwatchesBackground = GetThemeBackground();
        }

        private bool HasWork => _listDirty || _sortDirty || _controlDirty || _fontDirty;

        private void Mark(bool list = false, bool sort = false, bool control = false, bool font = false)
        {
            if (!_ready)
                return;

            if (list)
                _listDirty = true;
            if (sort)
                _sortDirty = true;
            if (control)
                _controlDirty = true;
            if (font)
                _fontDirty = true;
            StartDrain();
        }

        private void StartDrain()
        {
            if (_draining || !HasWork)
                return;

            _draining = true;
            _ = DrainAsync();
        }

        private async Task DrainAsync()
        {
            try
            {
                while (HasWork)
                {
                    var list = _listDirty;
                    var sort = _sortDirty;
                    var control = _controlDirty;
                    var font = _fontDirty;
                    _listDirty = _sortDirty = _controlDirty = _fontDirty = false;

                    if (list || sort || control)
                    {
                        await Task.Delay(PickerSettle);
                        if (HasWork)
                            continue;
                        await RunDataTransitionAsync(list, sort, control, font);
                    }
                    else
                    {
                        await RunFontTransitionAsync();
                    }
                }
            }
            finally
            {
                _draining = false;
                if (HasWork)
                    StartDrain();
            }
        }

        private async Task RunDataTransitionAsync(bool list, bool sort, bool control, bool font)
        {
            var headline = OperationTitle(list, sort, control);
            PresentTiming(headline);
            await NextTurnAsync();

            var total = Stopwatch.StartNew();
            List<ColorModel>? sorted = null;
            TimeSpan? sortTime = null;
            if (list || sort)
            {
                sorted = SelectedColorList.Colors.ToList();
                var sortWatch = Stopwatch.StartNew();
                Sort(sorted, SelectedSortOption.SortType, SelectedSortOption.Direction);
                sortWatch.Stop();
                sortTime = sortWatch.Elapsed;
            }

            var renderWatch = Stopwatch.StartNew();
            var items = (IReadOnlyList<ColorModel>?)sorted ?? Swatches.ToList();
            await PublishItemsAsync(items, SelectedControl == CollectionViewControl, font);
            renderWatch.Stop();
            total.Stop();
            PresentTiming(FormatTiming(headline, total.Elapsed, renderWatch.Elapsed, sortTime, font: null));
        }

        private async Task RunFontTransitionAsync()
        {
            var size = FontSizePreview;
            if (Math.Abs(SwatchFontSize - size) < 0.05)
                return;

            var headline = string.Create(CultureInfo.InvariantCulture, $"Set font to {size:0}");
            PresentTiming(headline);
            await NextTurnAsync();

            var total = Stopwatch.StartNew();
            var fontWatch = Stopwatch.StartNew();
            SwatchFontSize = size;
            await NextTurnAsync();
            fontWatch.Stop();
            total.Stop();
            PresentTiming(FormatTiming(headline, total.Elapsed, render: null, sort: null, fontWatch.Elapsed));
        }

        private async Task PublishItemsAsync(IReadOnlyList<ColorModel> items, bool collectionView, bool font)
        {
            if (collectionView)
            {
                if (font)
                    SwatchFontSize = FontSizePreview;
                Swatches = new ObservableCollection<ColorModel>(items);
                UseCollectionView = true;
                await NextTurnAsync();
                await ClearFlexInBatchesAsync();
                return;
            }

            await ClearFlexInBatchesAsync();
            if (font)
                SwatchFontSize = FontSizePreview;
            UseCollectionView = false;
            Swatches = new ObservableCollection<ColorModel>(items);
            await AddFlexInBatchesAsync(items);
        }

        private async Task ClearFlexInBatchesAsync()
        {
            while (_flexItems.Count > 0)
            {
                var count = Math.Min(FlexBatchSize, _flexItems.Count);
                for (var i = 0; i < count; i++)
                    _flexItems.RemoveAt(_flexItems.Count - 1);
                await NextTurnAsync();
            }
        }

        private async Task AddFlexInBatchesAsync(IReadOnlyList<ColorModel> items)
        {
            for (var start = 0; start < items.Count; start += FlexBatchSize)
            {
                var end = Math.Min(start + FlexBatchSize, items.Count);
                for (var i = start; i < end; i++)
                    _flexItems.Add(items[i]);
                await NextTurnAsync();
            }
        }

        private string OperationTitle(bool list, bool sort, bool control)
        {
            if (list)
                return $"Swap to {SelectedColorList.Description} list";
            if (sort)
                return $"Sort by {SelectedSortOption.Name}";
            if (control)
                return $"Swap to {SelectedControl}";
            return string.Create(CultureInfo.InvariantCulture, $"Set font to {FontSizePreview:0}");
        }

        private void PresentTiming(string text)
        {
            if (!ShowTimings)
                return;

            TimingText = text;
        }

        private static string FormatTiming(string headline, TimeSpan total, TimeSpan? render, TimeSpan? sort, TimeSpan? font)
        {
            var parts = new List<string>(3);
            if (render is TimeSpan renderTime)
                parts.Add(string.Create(CultureInfo.InvariantCulture, $"render: {renderTime.TotalSeconds:0.000}"));
            if (sort is TimeSpan sortTime)
                parts.Add(string.Create(CultureInfo.InvariantCulture, $"sort: {sortTime.TotalSeconds:0.000}"));
            if (font is TimeSpan fontTime)
                parts.Add(string.Create(CultureInfo.InvariantCulture, $"font: {fontTime.TotalSeconds:0.000}"));

            var detail = string.Create(CultureInfo.InvariantCulture, $"{total.TotalSeconds:0.000} s");
            if (parts.Count > 0)
                detail = $"{detail} ({string.Join(", ", parts)})";
            return headline + "\n" + detail;
        }

        private static Task NextTurnAsync()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null)
                return Task.CompletedTask;

            return dispatcher.DispatchAsync(static () => { });
        }

        private static Color GetThemeBackground()
        {
            var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
            return dark
                ? ResourceColor("DarkBackground", "#222222")
                : ResourceColor("LightBackground", "#FFFFFF");
        }

        private static Color ResourceColor(string key, string fallback)
        {
            if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color)
                return color;

            return Color.FromArgb(fallback);
        }

        private static void Sort(List<ColorModel> colors, ColorSortType sortType, ListSortDirection sortDirection)
        {
            int direction = sortDirection.Equals(ListSortDirection.Ascending) ? 1 : -1;

            switch (sortType)
            {
                case ColorSortType.Luminocity:
                    colors.Sort((a, b) => direction * a.Color.GetLuminosity().CompareTo(b.Color.GetLuminosity()));
                    break;
                case ColorSortType.Saturation:
                    colors.Sort((a, b) => direction * a.Color.GetSaturation().CompareTo(b.Color.GetSaturation()));
                    break;
                case ColorSortType.Hue:
                    colors.Sort((a, b) => direction * a.Color.GetHue().CompareTo(b.Color.GetHue()));
                    break;
                case ColorSortType.Name:
                    colors.Sort((a, b) => direction * a.Name.CompareTo(b.Name));
                    break;
            }
        }
    }
}
