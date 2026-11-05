namespace MauiLab.Colors;

public partial class SwatchesView : ContentView
{
    const double PreferredCardWidth = 180;
    const double PreferredGap = 36;
    const double MinimumGap = 8;
    const double CardHeight = 120;

    public static readonly BindableProperty CardWidthProperty = BindableProperty.Create(
        nameof(CardWidth), typeof(double), typeof(SwatchesView), PreferredCardWidth);

    public static readonly BindableProperty CardMarginProperty = BindableProperty.Create(
        nameof(CardMargin), typeof(Thickness), typeof(SwatchesView), new Thickness(PreferredGap / 2));

    private double _measuredWidth = -1;
    private SwatchMetrics? _metrics;
    private SwatchMetrics? _nativeMetrics;
    private bool _applyingLayout;

    public SwatchesView()
    {
        InitializeComponent();
        SwatchesCollection.HandlerChanged += (_, _) =>
        {
            _nativeMetrics = null;
            if (_metrics is SwatchMetrics metrics)
                ApplyPlatformLayout(metrics);
        };
    }

    public double CardWidth
    {
        get => (double)GetValue(CardWidthProperty);
        set => SetValue(CardWidthProperty, value);
    }

    public Thickness CardMargin
    {
        get => (Thickness)GetValue(CardMarginProperty);
        set => SetValue(CardMarginProperty, value);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 1 || Math.Abs(_measuredWidth - width) < 0.5)
            return;

        _measuredWidth = width;
        var metrics = SwatchMetrics.Measure(width);
        _metrics = metrics;

        if (SwatchGridLayout.Span != metrics.Span)
            SwatchGridLayout.Span = metrics.Span;
        if (Math.Abs(CardWidth - metrics.CardWidth) > 0.5)
            CardWidth = metrics.CardWidth;

        var margin = new Thickness(metrics.Gap / 2);
        if (CardMargin != margin)
            CardMargin = margin;

        ApplyPlatformLayout(metrics);
        // Span updates replace the native layout. Reapply after that turn so the inset wins.
        Dispatcher.Dispatch(() =>
        {
            if (_metrics is SwatchMetrics current)
                ApplyPlatformLayout(current, force: true);
        });
    }

    // CollectionView has no Padding in this MAUI build. The platform scroll view is full width,
    // and the content inset keeps the scrollbar on the trailing edge.
    private void ApplyPlatformLayout(SwatchMetrics metrics, bool force = false)
    {
        if (_applyingLayout)
            return;
        if (!force && _nativeMetrics is SwatchMetrics applied && applied.Equals(metrics))
            return;

        _applyingLayout = true;
        try
        {
            ApplyPlatformLayoutCore(metrics);
        }
        finally
        {
            _applyingLayout = false;
        }
    }

    private void ApplyPlatformLayoutCore(SwatchMetrics metrics)
    {

#if IOS || MACCATALYST
        if (SwatchesCollection.Handler?.PlatformView is not UIKit.UICollectionView collectionView)
            return;

        var padding = metrics.ContentPadding;
        var itemHeight = (nfloat)(CardHeight + metrics.Gap);
        var itemSize = UIKit.NSCollectionLayoutSize.Create(
            UIKit.NSCollectionLayoutDimension.CreateFractionalWidth(1f / metrics.Span),
            UIKit.NSCollectionLayoutDimension.CreateAbsolute(itemHeight));
        var item = UIKit.NSCollectionLayoutItem.Create(itemSize);
        var groupSize = UIKit.NSCollectionLayoutSize.Create(
            UIKit.NSCollectionLayoutDimension.CreateFractionalWidth(1),
            UIKit.NSCollectionLayoutDimension.CreateAbsolute(itemHeight));
        var group = UIKit.NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, metrics.Span);
        group.InterItemSpacing = UIKit.NSCollectionLayoutSpacing.CreateFixed(0);
        var section = UIKit.NSCollectionLayoutSection.Create(group);
        section.InterGroupSpacing = 0;
        section.ContentInsets = new UIKit.NSDirectionalEdgeInsets(
            (nfloat)padding.Top,
            (nfloat)padding.Left,
            (nfloat)padding.Bottom,
            (nfloat)padding.Right);
        if (OperatingSystem.IsIOSVersionAtLeast(16) || OperatingSystem.IsMacCatalystVersionAtLeast(16))
            section.ContentInsetsReference = UIKit.UIContentInsetsReference.None;

        collectionView.SetCollectionViewLayout(new UIKit.UICollectionViewCompositionalLayout(section), false);
#elif ANDROID
        if (SwatchesCollection.Handler?.PlatformView is not AndroidX.RecyclerView.Widget.RecyclerView recycler)
            return;

        var padding = metrics.ContentPadding;
        var density = DeviceDisplay.Current.MainDisplayInfo.Density;
        int Px(double dip) => (int)Math.Round(dip * density);
        recycler.SetClipToPadding(false);
        recycler.SetPadding(Px(padding.Left), Px(padding.Top), Px(padding.Right), Px(padding.Bottom));
        recycler.ScrollBarStyle = Android.Views.ScrollbarStyles.InsideOverlay;
        recycler.RequestLayout();
#else
        _ = metrics;
#endif
        _nativeMetrics = metrics;
    }

    private readonly record struct SwatchMetrics(int Span, double CardWidth, double Gap, double Side)
    {
        public Thickness ContentPadding
        {
            get
            {
                var horizontal = Math.Max(0, Side - Gap / 2);
                // A point of slack keeps rounding from wrapping the last column. Both sides stay equal.
                if (horizontal >= 1)
                    horizontal -= 0.5;
                return new Thickness(horizontal, Gap / 2);
            }
        }

        public static SwatchMetrics Measure(double width)
        {
            var span = Math.Max(2, (int)Math.Floor((width + PreferredGap) / (PreferredCardWidth + PreferredGap)));
            var card = PreferredCardWidth;
            var gap = PreferredGap;
            var side = SideOf(width, span, card, gap);

            if (side < 0)
            {
                var gapForPreferredCards = (width - span * card) / (span - 1);
                if (gapForPreferredCards >= MinimumGap)
                    gap = gapForPreferredCards;
                else
                {
                    gap = MinimumGap;
                    card = (width - (span - 1) * gap) / span;
                }

                side = SideOf(width, span, card, gap);
            }

            // Flex padding is side minus half the gap, so the side margin has to be at least half the gap.
            if (side < gap / 2)
            {
                var gapMatchingHalfSide = (width - span * card) / span;
                if (gapMatchingHalfSide >= MinimumGap && gapMatchingHalfSide <= gap)
                    gap = gapMatchingHalfSide;
                else
                {
                    gap = MinimumGap;
                    card = (width - span * gap) / span;
                }

                side = SideOf(width, span, card, gap);
            }

            if (card < 1)
                card = 1;
            if (side < 0)
                side = 0;

            return new SwatchMetrics(span, card, gap, side);
        }

        private static double SideOf(double width, int span, double card, double gap) =>
            (width - span * card - (span - 1) * gap) / 2;
    }
}
