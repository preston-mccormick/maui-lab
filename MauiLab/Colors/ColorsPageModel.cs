using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace MauiLab.Colors
{
    public class ColorsPageModel : ObservableObject
    {
        private ColorModel? _selectedBackgroundColor;
        private ColorSortOption _selectedSortOption;

        public ColorsPageModel()
        {
            Swatches = new ObservableCollection<ColorModel>(GetNamedColors() );
            Title = "MAUI Named Colors";
            SortOptions = new List<ColorSortOption>(ColorSortOption.GetColorSortOptions());
            _selectedSortOption = SortOptions.First();

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
                {
                    _ = SortSwatches(value);
                }
            }
        }

        public ObservableCollection<ColorModel> Swatches { get; }

        public List<ColorSortOption> SortOptions { get; }

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
                    Color c = (Color)field.GetValue(null);
                    var cm = new ColorModel { Name = field.Name, Color = c };
                    swatches.Add(cm);
                }
            }

            swatches.Sort();

            return swatches;
        }

        private async Task SortSwatches(ColorSortOption sortOption)
        {
            if (sortOption == null) return;
            await SortSwatches(sortOption.SortType, sortOption.Direction);
        }

        private async Task SortSwatches(ColorSortType sortType, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            int direction = sortDirection.Equals(ListSortDirection.Ascending) ? 1 : -1;

            switch (sortType)
            {
                case ColorSortType.Luminocity:
                    await SortInPlace(Swatches, (a, b) =>
                    {
                        var aa = a.Color.GetLuminosity();
                        var bb = b.Color.GetLuminosity();
                        return direction * aa.CompareTo(bb);
                    });
                    break;

                case ColorSortType.Saturation:
                    await SortInPlace(Swatches, (a, b) => direction * a.Color.GetSaturation().CompareTo(b.Color.GetSaturation()));
                    break;

                case ColorSortType.Hue:
                    await SortInPlace(Swatches, (a, b) => direction * a.Color.GetHue().CompareTo(b.Color.GetHue()));
                    break;

                case ColorSortType.Name:
                    await SortInPlace(Swatches, (a, b) => direction * a.Name.CompareTo(b.Name));
                    break;
            }
        }

        private static async Task SortInPlace<T>(ObservableCollection<T> collection, Comparison<T>? comparison = null)
        {
            var sortableList = new List<T>(collection);

            if (comparison == null) sortableList.Sort();
            else sortableList.Sort(comparison);

            for (var i = 0; i < sortableList.Count; i++)
            {
                T item = sortableList[i];
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var oldIndex = collection.IndexOf(item);
                    if (oldIndex != i)
                        collection.Move(oldIndex, i);
                });
            }
        }
    }
}