#if IOS || MACCATALYST
using AVFoundation;
using Foundation;
#endif
#if WINDOWS
using Windows.Media.Core;
using Windows.Media.Playback;
#endif

namespace MauiLab.MenuButton.Controls;

/// <summary>
/// Quiet click played when a menu row highlights. Platforms without a player, including
/// the plain net11.0 build used by tests, do nothing.
/// </summary>
static class MenuClick
{
    const float Volume = 0.08f;

    static readonly byte[] ClickWav = BuildWav();

    public static void Play()
    {
        try
        {
#if IOS || MACCATALYST
            PlayApple();
#elif ANDROID
            PlayAndroid();
#elif WINDOWS
            PlayWindows();
#endif
        }
        catch (Exception)
        {
            // A missing or rejected sound should not stop the menu from opening.
        }
    }

#if IOS || MACCATALYST || ANDROID || WINDOWS
    static string? cachedPath;

    static string PathToClick()
    {
        if (cachedPath is not null)
            return cachedPath;

        var path = Path.Combine(FileSystem.CacheDirectory, "menu-hover-tick.wav");
        if (!File.Exists(path))
            File.WriteAllBytes(path, ClickWav);
        cachedPath = path;
        return path;
    }
#endif

#if IOS || MACCATALYST
    static AVAudioPlayer? applePlayer;
    static bool sessionReady;

    static void PlayApple()
    {
        if (!sessionReady)
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Ambient);
            sessionReady = true;
        }

        if (applePlayer is null)
        {
            applePlayer = AVAudioPlayer.FromUrl(NSUrl.FromFilename(PathToClick()), out _);
            if (applePlayer is null)
                return;

            applePlayer.Volume = Volume;
            applePlayer.PrepareToPlay();
        }

        applePlayer.CurrentTime = 0;
        applePlayer.Play();
    }
#elif ANDROID
    static Android.Media.MediaPlayer? androidPlayer;

    static void PlayAndroid()
    {
        if (androidPlayer is null)
        {
            androidPlayer = new Android.Media.MediaPlayer();
            androidPlayer.SetDataSource(PathToClick());
            androidPlayer.Prepare();
            androidPlayer.SetVolume(Volume, Volume);
        }
        else
        {
            androidPlayer.SeekTo(0);
        }

        androidPlayer.Start();
    }
#elif WINDOWS
    static MediaPlayer? windowsPlayer;

    static void PlayWindows()
    {
        if (windowsPlayer is null)
        {
            windowsPlayer = new MediaPlayer { Volume = Volume };
            windowsPlayer.Source = MediaSource.CreateFromUri(new Uri(PathToClick()));
            windowsPlayer.Play();
            return;
        }

        windowsPlayer.PlaybackSession.Position = TimeSpan.Zero;
        windowsPlayer.Play();
    }
#endif

    static byte[] BuildWav()
    {
        const int sampleRate = 22050;
        const int samples = 240;
        var pcm = new byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var t = i / (double)sampleRate;
            var envelope = Math.Exp(-t * 520);
            var tone = Math.Sin(2 * Math.PI * 880 * t);
            var sample = tone * envelope * 0.28;
            var value = (short)Math.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
            pcm[i * 2] = (byte)(value & 0xff);
            pcm[i * 2 + 1] = (byte)((value >> 8) & 0xff);
        }

        const int header = 44;
        var file = new byte[header + pcm.Length];
        WriteAscii(file, 0, "RIFF");
        WriteInt(file, 4, 36 + pcm.Length);
        WriteAscii(file, 8, "WAVE");
        WriteAscii(file, 12, "fmt ");
        WriteInt(file, 16, 16);
        WriteShort(file, 20, 1);
        WriteShort(file, 22, 1);
        WriteInt(file, 24, sampleRate);
        WriteInt(file, 28, sampleRate * 2);
        WriteShort(file, 32, 2);
        WriteShort(file, 34, 16);
        WriteAscii(file, 36, "data");
        WriteInt(file, 40, pcm.Length);
        pcm.CopyTo(file, header);
        return file;
    }

    static void WriteAscii(byte[] file, int offset, string text)
    {
        for (var i = 0; i < text.Length; i++)
            file[offset + i] = (byte)text[i];
    }

    static void WriteInt(byte[] file, int offset, int value)
    {
        file[offset] = (byte)value;
        file[offset + 1] = (byte)(value >> 8);
        file[offset + 2] = (byte)(value >> 16);
        file[offset + 3] = (byte)(value >> 24);
    }

    static void WriteShort(byte[] file, int offset, short value)
    {
        file[offset] = (byte)value;
        file[offset + 1] = (byte)(value >> 8);
    }
}
