using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiLab.Colors
{
    public partial class ColorsPageModel : ObservableObject
    {
        private ColorModel? _selectedBackgroundColor;
        private ColorSortOption _selectedSortOption;

        public RelayCommand<ColorModel> SwatchTappedCommand { get; }

        public ColorsPageModel()
        {
            SwatchTappedCommand = new RelayCommand<ColorModel>(SwatchTapped);
            ColorLists = ColorCatalog.Load();
            SelectedColorList = ColorLists[0];
            SortOptions = ColorSortOption.GetColorSortOptions();
            _selectedSortOption = SortOptions[0];
            SwatchesBackground = ThemeBackground();
            if (Application.Current is Application app)
                app.RequestedThemeChanged += OnRequestedThemeChanged;
            ReplaceSwatches();
        }

        public string Title { get; } = "Named Colors";

        public IReadOnlyList<ColorListOption> ColorLists { get; }

        public List<ColorSortOption> SortOptions { get; }

        [ObservableProperty]
        public partial ColorListOption SelectedColorList { get; set; }

        public ColorSortOption SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (SetProperty(ref _selectedSortOption, value))
                    ReplaceSwatches();
            }
        }

        [ObservableProperty]
        public partial ObservableCollection<ColorModel> Swatches { get; set; } = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontSizeText))]
        public partial double FontSizePreview { get; set; } = 14;

        [ObservableProperty]
        public partial double SwatchFontSize { get; set; } = 14;

        public string FontSizeText => FontSizePreview.ToString("0", CultureInfo.InvariantCulture);

        [ObservableProperty]
        public partial Color SwatchesBackground { get; set; }

        public ColorModel? SelectedBackgroundColor
        {
            get => _selectedBackgroundColor;
            set => SetProperty(ref _selectedBackgroundColor, value);
        }

        public void CommitFontSize()
        {
            var size = Math.Clamp(FontSizePreview, 8, 40);
            if (Math.Abs(size - FontSizePreview) > 0.05)
                FontSizePreview = size;
            if (Math.Abs(SwatchFontSize - size) < 0.05)
                return;

            SwatchFontSize = size;
        }

        public void Reset()
        {
            SelectedColorList = ColorLists[0];
            SelectedSortOption = SortOptions[0];
            FontSizePreview = 14;
            SwatchFontSize = 14;
            SelectedBackgroundColor = null;
            SwatchesBackground = ThemeBackground();
            ReplaceSwatches();
        }

        partial void OnSelectedColorListChanged(ColorListOption value) => ReplaceSwatches();

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
                SwatchesBackground = ThemeBackground();
        }

        private void ReplaceSwatches()
        {
            if (SelectedColorList == null || _selectedSortOption == null)
                return;

            var colors = SelectedColorList.Colors.ToList();
            Sort(colors, _selectedSortOption.SortType, _selectedSortOption.Direction);
            Swatches = new ObservableCollection<ColorModel>(colors);
        }

        private static Color ThemeBackground()
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
            var direction = sortDirection == ListSortDirection.Ascending ? 1 : -1;
            colors.Sort((a, b) => direction * (sortType switch
            {
                ColorSortType.Luminocity => a.Color.GetLuminosity().CompareTo(b.Color.GetLuminosity()),
                ColorSortType.Saturation => a.Color.GetSaturation().CompareTo(b.Color.GetSaturation()),
                ColorSortType.Hue => a.Color.GetHue().CompareTo(b.Color.GetHue()),
                _ => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture)
            }));
        }
    }
}
