namespace MauiLab.Usage;

public class UsageRailView : GraphicsView
{
    public static readonly BindableProperty RailProperty = BindableProperty.Create(
        nameof(Rail),
        typeof(TimelineRail),
        typeof(UsageRailView),
        null,
        propertyChanged: (bindable, _, _) => ((UsageRailView)bindable).Invalidate());

    public UsageRailView()
    {
        InputTransparent = true;
        BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent;
        Drawable = new RailDrawable(this);
    }

    public TimelineRail? Rail
    {
        get => (TimelineRail?)GetValue(RailProperty);
        set => SetValue(RailProperty, value);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        if (Application.Current == null)
            return;

        Application.Current.RequestedThemeChanged -= OnThemeChanged;
        Application.Current.RequestedThemeChanged += OnThemeChanged;
    }

    void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();

    sealed class RailDrawable : IDrawable
    {
        readonly UsageRailView _view;

        public RailDrawable(UsageRailView view) => _view = view;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var rail = _view.Rail;
            if (rail == null || dirtyRect.Height <= 0)
                return;

            var height = dirtyRect.Height;
            var mid = height / 2f;
            float X(int lane) => 14 + lane * 22;

            canvas.StrokeColor = UsageColors.Themed("Gray", "LighterGray", Microsoft.Maui.Graphics.Colors.Gray);
            canvas.StrokeSize = 2;
            canvas.StrokeLineCap = LineCap.Round;

            foreach (var lane in rail.VerticalLanes)
                canvas.DrawLine(X(lane), 0, X(lane), height);

            var x = X(rail.Depth);
            if (rail.LineFromTop)
                canvas.DrawLine(x, 0, x, mid);
            if (rail.LineToBottom)
                canvas.DrawLine(x, mid, x, height);
            if (rail.CurveToLane is int target)
            {
                var path = new PathF();
                path.MoveTo(x, mid);
                path.QuadTo(x, height, X(target), height);
                canvas.DrawPath(path);
            }

            canvas.FillColor = UsageColors.ForCategory(rail.Category);
            canvas.FillCircle(x, mid, 6);
        }
    }
}
