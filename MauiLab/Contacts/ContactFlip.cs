namespace MauiLab.Contacts;

static class ContactFlip
{
    public const uint HalfMilliseconds = 200;

    public static void Prepare(VisualElement face)
    {
#if IOS || MACCATALYST
        if (face.Handler?.PlatformView is UIKit.UIView native)
        {
            var transform = native.Layer.Transform;
            transform.M34 = -1f / 800f;
            native.Layer.Transform = transform;
        }
#elif ANDROID
        if (face.Handler?.PlatformView is Android.Views.View native)
        {
            var density = native.Resources?.DisplayMetrics?.Density ?? 1f;
            native.CameraDistance = 8000 * density;
        }
#endif
    }

    public static Task TurnOut(VisualElement face)
    {
#if WINDOWS
        return face.ScaleXToAsync(0, HalfMilliseconds, Easing.SinIn);
#else
        return Animate(face, face.RotationY, 90, Easing.SinIn);
#endif
    }

    public static void Park(VisualElement face)
    {
        face.AbortAnimation("contact-flip");
#if WINDOWS
        face.ScaleX = 0;
#else
        face.RotationY = -90;
        Prepare(face);
#endif
    }

    public static Task TurnIn(VisualElement face)
    {
#if WINDOWS
        return face.ScaleXToAsync(1, HalfMilliseconds, Easing.SinOut);
#else
        return Animate(face, face.RotationY, 0, Easing.SinOut);
#endif
    }

    static Task Animate(VisualElement face, double from, double to, Easing easing)
    {
        var done = new TaskCompletionSource();
        face.AbortAnimation("contact-flip");
        new Animation(value =>
        {
            face.RotationY = value;
            Prepare(face);
        }, from, to, easing).Commit(
            face,
            "contact-flip",
            length: HalfMilliseconds,
            finished: (_, _) => done.TrySetResult());
        return done.Task;
    }

    public static void Flatten(VisualElement face)
    {
        face.RotationY = 0;
        face.ScaleX = 1;
    }
}
