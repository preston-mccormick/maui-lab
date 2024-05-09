using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace MauiLab.Colors
{
    public partial class ColorsPageModel : ObservableObject
    {
        private ColorModel? _selectedBackgroundColor;
        private ColorSortOption _selectedSortOption;

        public ColorsPageModel()
        {
            Swatches = new ObservableCollection<ColorModel>(GetNamedColors());
            Title = "MAUI Named Colors";
            SortOptions = new List<ColorSortOption>(ColorSortOption.GetColorSortOptions());
            _selectedSortOption = SortOptions.First();
            PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedSortOption))
                    await HandleSelectedSortOptionChanged();
            };
        }

        public ColorModel? SelectedBackgroundColor
        {
            get => _selectedBackgroundColor;
            set => SetProperty(ref _selectedBackgroundColor, value);
        }

        public ColorSortOption SelectedSortOption
        {
            get => _selectedSortOption;
            set => SetProperty(ref _selectedSortOption, value);
        }
        public ReadOnlyCollection<string> UpdateSwatchesStrategy { get; } = new ReadOnlyCollection<string>(
            ["Replace list", "Move items", "Replace list on main thread", "Move items on main thread", "Batch move items on main thread"]);

        [ObservableProperty]
        private string selectedUpdateSwatchesStrategy = "Replace list";

        [ObservableProperty]
        private ObservableCollection<ColorModel> swatches;

        [ObservableProperty]
        private List<ColorSortOption> sortOptions;

        public string Title { get; set; }

        public static List<ColorModel> GetNamedColors()
        {
            var swatches = new List<ColorModel>();

            Type mauiColors = typeof(Microsoft.Maui.Graphics.Colors);
            Type mauiColor = typeof(Color);
            var fields = mauiColors.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                if (field.FieldType == mauiColor)
                {
                    object? value = field.GetValue(null);
                    if (value == null) continue;
                    var color = (Color)value;
                    var swatch = new ColorModel(color, field.Name);
                    swatches.Add(swatch);
                }
            }

            swatches.Sort();

            return swatches;
        }

        private async Task HandleSelectedSortOptionChanged()
        {
            await SortSwatches(SelectedSortOption.SortType, SelectedSortOption.Direction);
        }

        private async Task SortSwatches(ColorSortType sortType, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            int direction = sortDirection.Equals(ListSortDirection.Ascending) ? 1 : -1;

            switch (sortType)
            {
                case ColorSortType.Luminocity:
                    await SortSwatches((a, b) =>
                    {
                        var aa = a.Color.GetLuminosity();
                        var bb = b.Color.GetLuminosity();
                        return direction * aa.CompareTo(bb);
                    });
                    break;

                case ColorSortType.Saturation:
                    await SortSwatches((a, b) => direction * a.Color.GetSaturation().CompareTo(b.Color.GetSaturation()));
                    break;

                case ColorSortType.Hue:
                    await SortSwatches((a, b) => direction * a.Color.GetHue().CompareTo(b.Color.GetHue()));
                    break;

                case ColorSortType.Name:
                    await SortSwatches((a, b) => direction * a.Name.CompareTo(b.Name));
                    break;
            }
        }

        private async Task SortSwatches(Comparison<ColorModel>? comparison = null)
        {
            var sortedSwatches = Swatches.ToList();
            if (comparison == null) sortedSwatches.Sort();
            else sortedSwatches.Sort(comparison);

            await UpdateSwatches(sortedSwatches, SelectedUpdateSwatchesStrategy);

        }

        private async Task UpdateSwatches(IList<ColorModel> sortedSwatches, string strategy)
        {
            switch (strategy)
            {
                case "Replace list":
                    Swatches = new ObservableCollection<ColorModel>(sortedSwatches);
                    break;
                case "Move items":
                    for (var i = 0; i < sortedSwatches.Count; i++)
                    {
                        ColorModel item = sortedSwatches[i];
                        var oldIndex = Swatches.IndexOf(item);
                        if (oldIndex != i)
                            Swatches.Move(oldIndex, i);
                    }
                    break;
                case "Replace list on main thread":
                    await MainThread.InvokeOnMainThreadAsync(() => Swatches = new ObservableCollection<ColorModel>(sortedSwatches));
                    break;
                case "Move items on main thread":
                    for (var i = 0; i < sortedSwatches.Count; i++)
                    {
                        ColorModel item = sortedSwatches[i];
                        var oldIndex = Swatches.IndexOf(item);
                        if (oldIndex != i)
                        {
                            await MainThread.InvokeOnMainThreadAsync(() => Swatches.Move(oldIndex, i));
                        }
                    }
                    break;
                case "Batch move items on main thread":
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await UpdateSwatches(sortedSwatches, "Update");
                    });
                    break;
            }
        }

    }

}
