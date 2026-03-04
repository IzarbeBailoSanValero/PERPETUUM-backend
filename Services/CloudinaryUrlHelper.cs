namespace PERPETUUM.Services;

/// <summary>
/// Añade transformación 1:1 centro a URLs de Cloudinary para que retratos queden bien en la web.
/// </summary>
public static class CloudinaryUrlHelper
{
    private const string Transform1x1 = "c_fill,g_center,ar_1:1,w_600";

    /// <summary>
    /// Si la URL es de Cloudinary (image/upload/), inserta recorte 1:1 centro. Si no, devuelve la URL sin cambios.
    /// </summary>
    public static string To1x1Url(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url ?? string.Empty;
        if (!url.Contains("/image/upload/", StringComparison.OrdinalIgnoreCase)) return url;
        return url.Replace("/image/upload/", $"/image/upload/{Transform1x1}/", StringComparison.OrdinalIgnoreCase);
    }
}
