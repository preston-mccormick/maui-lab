using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;

namespace MauiLab.Layouts;

public enum LayoutItemKind
{
    Rectangle,
    Ellipse,
    Triangle,
    Star,
    Image,
    Panel,
    Stack
}

public enum LayoutDock
{
    None,
    Top,
    Left,
    Right,
    Bottom
}

public partial class LayoutItem : ObservableObject
{
    public LayoutItem(LayoutItemKind kind, string name, Color color)
    {
        Kind = kind;
        Name = name;
        Color = color;
    }

    public LayoutItemKind Kind { get; }

    public string Name { get; }

    [ObservableProperty]
    public partial Color Color { get; set; }

    [ObservableProperty]
    public partial int ZIndex { get; set; }

    [ObservableProperty]
    public partial double Rotation { get; set; }

    [ObservableProperty]
    public partial double Opacity { get; set; } = 1;

    [ObservableProperty]
    public partial bool IsClipped { get; set; }

    [ObservableProperty]
    public partial LayoutOptions HorizontalOptions { get; set; } = LayoutOptions.Fill;

    [ObservableProperty]
    public partial LayoutOptions VerticalOptions { get; set; } = LayoutOptions.Fill;

    [ObservableProperty]
    public partial string Text { get; set; } = "";

    [ObservableProperty]
    public partial int Row { get; set; }

    [ObservableProperty]
    public partial int Column { get; set; }

    [ObservableProperty]
    public partial int RowSpan { get; set; } = 1;

    [ObservableProperty]
    public partial int ColumnSpan { get; set; } = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(X))]
    [NotifyPropertyChangedFor(nameof(Y))]
    [NotifyPropertyChangedFor(nameof(Width))]
    [NotifyPropertyChangedFor(nameof(Height))]
    public partial Rect Bounds { get; set; }

    public double X
    {
        get => Bounds.X;
        set => Bounds = new Rect(Finite(value), Bounds.Y, Bounds.Width, Bounds.Height);
    }

    public double Y
    {
        get => Bounds.Y;
        set => Bounds = new Rect(Bounds.X, Finite(value), Bounds.Width, Bounds.Height);
    }

    public double Width
    {
        get => Bounds.Width;
        set => Bounds = new Rect(Bounds.X, Bounds.Y, Finite(value), Bounds.Height);
    }

    public double Height
    {
        get => Bounds.Height;
        set => Bounds = new Rect(Bounds.X, Bounds.Y, Bounds.Width, Finite(value));
    }

    [ObservableProperty]
    public partial AbsoluteLayoutFlags Flags { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Margin))]
    public partial double ItemMargin { get; set; }

    public Thickness Margin => new(Finite(ItemMargin));

    /// <summary>Children of a <see cref="LayoutItemKind.Stack"/> item.</summary>
    public ObservableCollection<LayoutItem> Children { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHorizontal))]
    public partial bool IsVertical { get; set; } = true;

    public bool IsHorizontal => !IsVertical;

    [ObservableProperty]
    public partial double Spacing { get; set; } = 8;

    public bool IsStack => Kind == LayoutItemKind.Stack;

    static double Finite(double value) => double.IsFinite(value) ? value : 0;

    [ObservableProperty]
    public partial double Grow { get; set; }

    [ObservableProperty]
    public partial double Shrink { get; set; } = 1;

    [ObservableProperty]
    public partial double Basis { get; set; }

    [ObservableProperty]
    public partial bool BasisIsPercent { get; set; }

    [ObservableProperty]
    public partial int Order { get; set; }

    [ObservableProperty]
    public partial FlexAlignSelf AlignSelf { get; set; } = FlexAlignSelf.Auto;

    [ObservableProperty]
    public partial LayoutDock Dock { get; set; } = LayoutDock.Left;

    [ObservableProperty]
    public partial double? Angle { get; set; }

    [ObservableProperty]
    public partial double WidthRequest { get; set; } = -1;

    [ObservableProperty]
    public partial double HeightRequest { get; set; } = -1;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public string ImageName { get; init; } = "rose.png";

    public static LayoutItem Create(LayoutItemKind kind, int index, Rect bounds, double width = 72, double height = 72, string? imageName = null) =>
        new(kind, kind.ToString(), Color.FromHsla(index * 0.17f % 1f, 0.7f, 0.55f))
        {
            Bounds = bounds,
            WidthRequest = width,
            HeightRequest = height,
            ImageName = imageName ?? Images[index % Images.Length]
        };

    static readonly string[] Images =
    [
        "rose.png", "apple.png", "book.png", "diamondsword.png",
        "gold_ingot.png", "sign.png", "steelpick.png", "stonehoe.png"
    ];

    /// <summary>The kinds a random add on the absolute page can pick.</summary>
    public static readonly LayoutItemKind[] RandomKinds =
    [
        LayoutItemKind.Rectangle, LayoutItemKind.Ellipse, LayoutItemKind.Triangle,
        LayoutItemKind.Star, LayoutItemKind.Image
    ];
}
