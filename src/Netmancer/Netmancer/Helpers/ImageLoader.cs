using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace Netmancer.Helpers;

/// <summary>
/// Attached property that loads an image from an HTTP/HTTPS URL asynchronously
/// and assigns the resulting <see cref="Bitmap"/> to the <see cref="Image.Source"/>.
/// Includes a simple in-memory cache so the same URL is only downloaded once.
/// </summary>
public static class ImageLoader
{
    private static readonly HttpClient _httpClient = new();
    private static readonly ConcurrentDictionary<string, Bitmap> _cache = new();

    public static readonly AttachedProperty<string?> SourceProperty =
        AvaloniaProperty.RegisterAttached<Image, string?>("Source", typeof(ImageLoader));

    static ImageLoader()
    {
        SourceProperty.Changed.AddClassHandler<Image>(OnSourceChanged);
    }

    public static string? GetSource(Image image) => image.GetValue(SourceProperty);
    public static void SetSource(Image image, string? value) => image.SetValue(SourceProperty, value);

    private static async void OnSourceChanged(Image image, AvaloniaPropertyChangedEventArgs e)
    {
        var url = e.NewValue as string;

        if (string.IsNullOrEmpty(url))
        {
            image.Source = null;
            return;
        }

        // Return cached bitmap immediately if available
        if (_cache.TryGetValue(url, out var cached))
        {
            image.Source = cached;
            return;
        }

        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            using var stream = new MemoryStream(bytes);
            var bitmap = new Bitmap(stream);
            _cache.TryAdd(url, bitmap);
            image.Source = bitmap;
        }
        catch
        {
            image.Source = null;
        }
    }
}




