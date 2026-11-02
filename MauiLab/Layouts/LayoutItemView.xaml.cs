using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Layouts;
using Microsoft.Maui.Layouts;

namespace MauiLab.Layouts;

/// <summary>A page that hosts <see cref="LayoutItemView"/> and receives its taps.</summary>
public interface ILayoutItemHost
{
    void ItemTapped(LayoutItemView view, LayoutItem item);
}

public partial class LayoutItemView : ContentView
{
    static readonly TimeSpan NestedTapWindow = TimeSpan.FromMilliseconds(150);
    static LayoutItemView? _lastTapped;
    static DateTime _lastTapAt;

    readonly DockPositionConverter _dock = new();
    readonly FlexBasisConverter _basis = new();
    LayoutItem? _item;

    public LayoutItemView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty CanDragProperty =
        BindableProperty.Create(nameof(CanDrag), typeof(bool), typeof(LayoutItemView), false, propertyChanged: OnCanDragChanged);

    /// <summary>A long press on this view starts a drag. The host handles <see cref="Dragged"/>.</summary>
    public bool CanDrag
    {
        get => (bool)GetValue(CanDragProperty);
        set => SetValue(CanDragProperty, value);
    }

    public event EventHandler<LongPressingEventArgs>? Dragged;

    LongPressGestureRecognizer? _drag;

    public LayoutItem? Item => _item;

    static void OnCanDragChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((LayoutItemView)bindable).SetDrag((bool)newValue);

    void SetDrag(bool canDrag)
    {
        if (canDrag && _drag == null)
        {
            _drag = new LongPressGestureRecognizer { MinimumPressDuration = 400, AllowableMovement = 20 };
            _drag.LongPressing += OnDragging;
            GestureRecognizers.Add(_drag);
            SuppressFlyoutPan();
        }
        else if (!canDrag && _drag != null)
        {
            _drag.LongPressing -= OnDragging;
            GestureRecognizers.Remove(_drag);
            _drag = null;
        }
    }

    partial void SuppressFlyoutPan();

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (CanDrag)
            SuppressFlyoutPan();
    }

    void OnDragging(object? sender, LongPressingEventArgs e)
    {
        // A short lift shows the hold took, without touching the item's bound opacity.
        Scale = e.Status is GestureStatus.Started or GestureStatus.Running ? 1.04 : 1;
        Dragged?.Invoke(this, e);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_item != null)
            _item.PropertyChanged -= OnItemPropertyChanged;

        _item = BindingContext as LayoutItem;
        if (_item != null)
            _item.PropertyChanged += OnItemPropertyChanged;
        ApplyLayoutFields();
    }

    void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LayoutItem.Dock) or nameof(LayoutItem.Basis) or nameof(LayoutItem.BasisIsPercent))
            ApplyLayoutFields();
    }

    void ApplyLayoutFields()
    {
        if (_item == null)
            return;

        DockLayout.SetDockPosition(this, (DockPosition)_dock.Convert(_item.Dock, typeof(DockPosition), null, CultureInfo.CurrentCulture));
        var basis = (FlexBasis)_basis.Convert([_item.Basis, _item.BasisIsPercent], typeof(FlexBasis), null!, CultureInfo.CurrentCulture);
        FlexLayout.SetBasis(this, basis);
    }

    void OnTapped(object? sender, TappedEventArgs e)
    {
        if (_item == null)
            return;

        // A tap on a nested item can also reach the stack around it. Keep the inner one.
        var now = DateTime.UtcNow;
        if (_lastTapped != null && now - _lastTapAt < NestedTapWindow && IsAncestorOf(_lastTapped))
            return;

        _lastTapped = this;
        _lastTapAt = now;
        FindHost()?.ItemTapped(this, _item);
    }

    bool IsAncestorOf(Element element)
    {
        for (var parent = element.Parent; parent != null; parent = parent.Parent)
        {
            if (parent == this)
                return true;
        }

        return false;
    }

    ILayoutItemHost? FindHost()
    {
        for (Element? parent = Parent; parent != null; parent = parent.Parent)
        {
            if (parent is ILayoutItemHost host)
                return host;
        }

        return null;
    }
}
