namespace MauiLab.Layouts;

public partial class NumberLever : ContentView
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(NumberLever), 0d, BindingMode.TwoWay, propertyChanged: OnScaleChanged);

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(double), typeof(NumberLever), 0d, propertyChanged: OnScaleChanged);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(double), typeof(NumberLever), 100d, propertyChanged: OnScaleChanged);

    public static readonly BindableProperty StepProperty = BindableProperty.Create(
        nameof(Step), typeof(double), typeof(NumberLever), 1d, propertyChanged: OnScaleChanged);

    public static readonly BindableProperty InlineProperty = BindableProperty.Create(
        nameof(Inline), typeof(bool), typeof(NumberLever), false);

    public static readonly BindableProperty SliderSpanProperty = BindableProperty.Create(
        nameof(SliderSpan), typeof(int), typeof(NumberLever), 0);

    public static readonly BindableProperty HasUnsetProperty = BindableProperty.Create(
        nameof(HasUnset), typeof(bool), typeof(NumberLever), false, propertyChanged: OnScaleChanged);

    public static readonly BindableProperty UnsetValueProperty = BindableProperty.Create(
        nameof(UnsetValue), typeof(double), typeof(NumberLever), -1d, propertyChanged: OnScaleChanged);

    public static readonly BindableProperty HasUnboundedProperty = BindableProperty.Create(
        nameof(HasUnbounded), typeof(bool), typeof(NumberLever), false, propertyChanged: OnScaleChanged);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(NumberLever), null, propertyChanged: OnPlaceholderChanged);

    bool _syncing;
    bool _fromEntry;
    bool _editing;

    public NumberLever()
    {
        InitializeComponent();
    }

    public event EventHandler<FocusEventArgs>? Editing;

    public event EventHandler? Committed;

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Rounds slider values to this increment. The default is 1, so the slider lands on whole numbers.</summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>Puts the entry and the slider on one row. Used when the lever is the whole row.</summary>
    public bool Inline
    {
        get => (bool)GetValue(InlineProperty);
        set => SetValue(InlineProperty, value);
    }

    /// <summary>
    /// How many parent columns the slider covers, ending at this lever. Zero spans the whole row.
    /// </summary>
    public int SliderSpan
    {
        get => (int)GetValue(SliderSpanProperty);
        set => SetValue(SliderSpanProperty, value);
    }

    public bool HasUnset
    {
        get => (bool)GetValue(HasUnsetProperty);
        set => SetValue(HasUnsetProperty, value);
    }

    public double UnsetValue
    {
        get => (double)GetValue(UnsetValueProperty);
        set => SetValue(UnsetValueProperty, value);
    }

    public bool HasUnbounded
    {
        get => (bool)GetValue(HasUnboundedProperty);
        set => SetValue(HasUnboundedProperty, value);
    }

    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Text => ValueEntry.Text;

    public NumberScale Range => new(Minimum, Maximum, Step, HasUnset, UnsetValue, HasUnbounded);

    static void OnScaleChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((NumberLever)bindable).ShowValue();

    static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((NumberLever)bindable).ValueEntry.Placeholder = newValue as string;

    void ShowValue()
    {
        if (ValueEntry == null || ValueSlider == null)
            return;

        var scale = Range;
        _syncing = true;
        if (!_fromEntry || !ValueEntry.IsFocused)
            ValueEntry.Text = scale.Format(Value);
        var position = scale.ToSlider(Value);
        if (Math.Abs(ValueSlider.Value - position) > 0.001)
            ValueSlider.Value = position;
        if (TickLayer != null && (TickLayer.Drawable is not TickMarks marks || !marks.Scale.Equals(scale)))
            TickLayer.Drawable = new TickMarks(scale);
        _syncing = false;
    }

    void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_syncing || !Range.TryParse(e.NewTextValue, out var parsed) || Same(parsed, Value))
            return;

        _fromEntry = true;
        Value = parsed;
        _fromEntry = false;
    }

    void OnSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_syncing || Math.Abs(e.NewValue - Range.ToSlider(Value)) < 0.05)
            return;

        var mapped = Range.FromSlider(e.NewValue);
        if (Same(mapped, Value))
            return;

        // Snapshot the old value before the binding writes the new one.
        if (!_editing)
        {
            _editing = true;
            Editing?.Invoke(this, new FocusEventArgs(this, true));
        }

        Value = mapped;
    }

    void OnDragStarted(object? sender, EventArgs e)
    {
        if (_editing)
            return;

        _editing = true;
        Editing?.Invoke(this, new FocusEventArgs(this, true));
    }

    void OnDragCompleted(object? sender, EventArgs e)
    {
        _editing = false;
        Committed?.Invoke(this, EventArgs.Empty);
    }

    void OnEntryFocused(object? sender, FocusEventArgs e) => Editing?.Invoke(this, e);

    void OnEntryUnfocused(object? sender, FocusEventArgs e)
    {
        _syncing = true;
        ValueEntry.Text = Range.Format(Value);
        _syncing = false;
        Committed?.Invoke(this, EventArgs.Empty);
    }

    void OnEntryCompleted(object? sender, EventArgs e) => Committed?.Invoke(this, e);

    void OnLoaded(object? sender, EventArgs e)
    {
        if (Inline)
            ArrangeInline();
        else
            Dispatcher.Dispatch(PlaceSlider);
    }

    void ArrangeInline()
    {
        Host.RowDefinitions.Clear();
        Host.ColumnSpacing = 8;
        Host.ColumnDefinitions.Clear();
        Host.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        Host.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        ValueEntry.VerticalOptions = LayoutOptions.Center;
        Grid.SetRow(SliderHost, 0);
        Grid.SetColumn(SliderHost, 0);
        Grid.SetColumn(ValueEntry, 1);
        ApplyEnabled();
    }

    void PlaceSlider()
    {
        if (SliderHost.Parent is not Layout host || Parent is not Grid grid || grid.ColumnDefinitions.Count == 0)
            return;

        var row = Grid.GetRow(this);
        var column = Grid.GetColumn(this);
        var columns = grid.ColumnDefinitions.Count;
        int start;
        int span;
        if (SliderSpan > 0)
        {
            span = Math.Min(SliderSpan, columns);
            start = Math.Max(0, column - (span - 1));
        }
        else
        {
            start = 0;
            span = columns;
        }

        host.Children.Remove(SliderHost);
        grid.Children.Add(SliderHost);
        Grid.SetRow(SliderHost, row + 1);
        Grid.SetColumn(SliderHost, start);
        Grid.SetColumnSpan(SliderHost, span);
        Grid.SetRowSpan(SliderHost, 1);
        ApplyEnabled();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(IsEnabled))
            ApplyEnabled();
    }

    void ApplyEnabled()
    {
        if (SliderHost != null)
            SliderHost.IsEnabled = IsEnabled;
    }

    void OnTickSizeChanged(object? sender, EventArgs e) => TickLayer.Invalidate();

    static bool Same(double left, double right) =>
        double.IsNaN(left) ? double.IsNaN(right)
        : double.IsPositiveInfinity(left) ? double.IsPositiveInfinity(right)
        : left == right;

    sealed class TickMarks : IDrawable
    {
        readonly NumberScale _scale;

        public TickMarks(NumberScale scale) => _scale = scale;

        public NumberScale Scale => _scale;

        public void Draw(ICanvas canvas, RectF dirty)
        {
            var ticks = _scale.TickValues();
            if (ticks.Count == 0 || dirty.Width <= 1)
                return;

            var inset = Math.Min(16f, dirty.Width / 4);
            var usable = dirty.Width - inset * 2;
            canvas.StrokeColor = Color.FromArgb("#8E8E93");
            canvas.StrokeSize = 1;
            foreach (var value in ticks)
            {
                var x = inset + (float)(_scale.ToSlider(value) / NumberScale.SliderEnd) * usable;
                canvas.DrawLine(x, 0, x, dirty.Height);
            }
        }
    }
}
