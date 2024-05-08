using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace MauiLab.Colors
{
    public class ColorsPageModel : ObservableObject
    {
        private ColorModel _selectedBackgroundColor;
        private ColorSortOption _selectedSwatchSort;

        public ColorsPageModel(IEnumerable<ColorModel>? swatches = null, string title = "MAUI Named Colors")
        {
            Swatches = new ObservableCollection<ColorModel>(swatches ?? GetNamedColors() );
            Title = title;
            SortSwatches(ColorSortType.Luminocity, ListSortDirection.Descending);
        }

        public List<ColorModel> BackgroundColorChoices { get; }

        public ColorModel SelectedBackgroundColor
        {
            get => _selectedBackgroundColor;
            set => SetProperty(ref _selectedBackgroundColor, value);
        }

        public ColorSortOption SelectedSwatchSort
        {
            get => _selectedSwatchSort;
            set
            {
                if (SetProperty(ref _selectedSwatchSort, value))
                {
                    SortSwatches(value);
                }
            }
        }

        public ObservableCollection<ColorModel> Swatches { get; }

        public List<ColorSortOption> SwatchSortChoices { get; }

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

        private void SortSwatches(ColorSortOption sortOption)
        {
            if (sortOption == null) return;
            SortSwatches(sortOption.SortType, sortOption.Direction);
        }

        private void SortSwatches(ColorSortType sortType, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            int direction = sortDirection.Equals(ListSortDirection.Ascending) ? 1 : -1;

            switch (sortType)
            {
                case ColorSortType.Luminocity:
                    SortInPlace(Swatches, (a, b) =>
                    {
                        var aa = a.Color.GetLuminosity();
                        var bb = b.Color.GetLuminosity();
                        return direction * aa.CompareTo(bb);
                    });
                    break;

                case ColorSortType.Saturation:
                    SortInPlace(Swatches, (a, b) => direction * a.Color.GetSaturation().CompareTo(b.Color.GetSaturation()));
                    break;

                case ColorSortType.Hue:
                    //SortInPlace(Swatches, (a, b) =>
                    //{
                    //    var aa = a.Color.GetHue() + a.Color.GetLuminosity();
                    //    var bb = b.Color.GetHue() + b.Color.GetLuminosity();
                    //    return direction * aa.CompareTo(bb);
                    //});
                    SortInPlace(Swatches, (a, b) => direction * a.Color.GetHue().CompareTo(b.Color.GetHue()));
                    break;

                case ColorSortType.Name:
                    SortInPlace(Swatches, (a, b) => direction * a.Name.CompareTo(b.Name));
                    break;
            }
        }

        private static void SortInPlace<T>(ObservableCollection<T> collection, Comparison<T> comparison = null)
        {
            var sortableList = new List<T>(collection);

            if (comparison == null) sortableList.Sort();
            else sortableList.Sort(comparison);

            for (var i = 0; i < sortableList.Count; i++)
            {
                var oldIndex = collection.IndexOf(sortableList[i]);
                var newIndex = i;
                if (oldIndex != newIndex)
                    collection.Move(oldIndex, newIndex);
            }
        }
    }
}