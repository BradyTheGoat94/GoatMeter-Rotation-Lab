using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aion2DPSPro.Rotation;

namespace Aion2DPSPro.Overlay;
public static class SkillIconImages
{
    static readonly Dictionary<int, ImageSource> cache = new();
    public static ImageSource? Find(AionClass? className, string? skill)
    {
        var identity = SkillIconCatalog.Find(className, skill);
        if (identity is null) return null;
        if (cache.TryGetValue(identity.SkillId, out var image)) return image;
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri($"pack://application:,,,/GoatMeter;component/{identity.ResourcePath}");
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            cache[identity.SkillId] = bitmap;
            return bitmap;
        }
        catch (System.IO.IOException) { return null; }
        catch (NotSupportedException) { return null; }
    }
}
