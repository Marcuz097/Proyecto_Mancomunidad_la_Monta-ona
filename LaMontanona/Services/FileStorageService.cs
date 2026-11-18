namespace LaMontanona.Services;

public interface IFileStorageService
{
    /// <summary>Guarda una foto en wwwroot/uploads/incidentes y devuelve la ruta relativa para la BD.</summary>
    Task<string?> GuardarFotoIncidenteAsync(IFormFile? archivo, CancellationToken ct = default);
}

public class FileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    private static readonly string[] Permitidas = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxBytes = 5 * 1024 * 1024;

    public async Task<string?> GuardarFotoIncidenteAsync(IFormFile? archivo, CancellationToken ct = default)
    {
        if (archivo is null || archivo.Length == 0) return null;
        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!Permitidas.Contains(ext)) throw new InvalidOperationException("Formato no permitido. Use JPG, PNG o WEBP.");
        if (archivo.Length > MaxBytes) throw new InvalidOperationException("La foto supera los 5 MB.");

        var carpeta = Path.Combine(env.WebRootPath, "uploads", "incidentes");
        Directory.CreateDirectory(carpeta);
        var nombre = $"{Guid.NewGuid():N}{ext}"; // nunca se usa el nombre original
        await using var fs = File.Create(Path.Combine(carpeta, nombre));
        await archivo.CopyToAsync(fs, ct);
        return $"/uploads/incidentes/{nombre}";
    }
}
