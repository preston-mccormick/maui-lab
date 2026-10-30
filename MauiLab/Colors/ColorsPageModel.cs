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

        public RelayCommand<ColorModel> SwatchTappedCommand => new(SwatchTapped);

        private void SwatchTapped(ColorModel? model)
        {
            SelectedBackgroundColor = model;
        }

        public ColorsPageModel()
        {
            Swatches = [.. ColorModel.GetNamedColors()];
            Title = "MAUI Named Colors";
            SortOptions = ColorSortOption.GetColorSortOptions();
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
        public partial string SelectedUpdateSwatchesStrategy { get; set; } = "Replace list";

        [ObservableProperty]
        public partial ObservableCollection<ColorModel> Swatches { get; set; }

        [ObservableProperty]
        public partial List<ColorSortOption> SortOptions { get; set; }

        public string Title { get; set; }


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

        private async Task UpdateSwatches(List<ColorModel> sortedSwatches, string strategy)
        {
            switch (strategy)
            {
                case "Replace list":
                    Swatches = [.. sortedSwatches];
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
                    await MainThread.InvokeOnMainThreadAsync(() => Swatches = [.. sortedSwatches]);
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
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        for (var i = 0; i < sortedSwatches.Count; i++)
                        {
                            ColorModel item = sortedSwatches[i];
                            var oldIndex = Swatches.IndexOf(item);
                            if (oldIndex != i)
                                Swatches.Move(oldIndex, i);
                        }
                    });
                    break;
            }
        }

    }

}
