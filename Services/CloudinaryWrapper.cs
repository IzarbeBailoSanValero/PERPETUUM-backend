using CloudinaryDotNet;

namespace PERPETUUM.Services;

/// <summary>
/// Wrapper que solo instancia Cloudinary cuando CLOUDINARY_URL es válida.
/// Permite que la app arranque sin Cloudinary (recuerdos de texto funcionan; solo falla subida de fotos).
/// </summary>
public class CloudinaryWrapper
{
    public Cloudinary? Instance { get; }

    public bool IsConfigured => Instance != null;

    public CloudinaryWrapper(string? cloudinaryUrl)
    {
        if (!string.IsNullOrWhiteSpace(cloudinaryUrl) &&
            cloudinaryUrl.StartsWith("cloudinary://", StringComparison.OrdinalIgnoreCase))
        {
            Instance = new Cloudinary(cloudinaryUrl);
            Instance.Api.Secure = true;
        }
        else
        {
            Instance = null;
        }
    }
}
