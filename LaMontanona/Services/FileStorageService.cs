using System.Text.RegularExpressions;
namespace LaMontanona.Services;

public interface IFileStorageService
{
    Task<string?> GuardarFotoAsync(Stream contenido, string nombreOriginal, long longitud, string subcarpeta, CancellationToken ct = default);
    Task<string?> GuardarFotoIncidenteAsync(IFormFile? archivo, CancellationToken ct = default);
    /// <summary>Comprueba que la ruta sea una foto que subimos nosotros a esa carpeta y que exista.</summary>
    bool EsRutaValida(string? ruta, string subcarpeta);
    void Eliminar(string? rutaRelativa);
}

public partial class FileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    private static readonly string[] Extensiones = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] Subcarpetas = ["incidentes", "reportes", "contenido"];
    private const long MaxBytes = 5 * 1024 * 1024;

    [GeneratedRegex(@"^[0-9a-f]{32}\.(jpg|jpeg|png|webp)$")]
    private static partial Regex NombreGenerado();

    public Task<string?> GuardarFotoIncidenteAsync(IFormFile? archivo, CancellationToken ct = default) =>
        archivo is null || archivo.Length == 0
            ? Task.FromResult<string?>(null)
            : GuardarFotoAsync(archivo.OpenReadStream(), archivo.FileName, archivo.Length, "incidentes", ct);

    public async Task<string?> GuardarFotoAsync(Stream contenido, string nombreOriginal, long longitud, string subcarpeta, CancellationToken ct = default)
    {
        if (longitud == 0) return null;
        if (!Subcarpetas.Contains(subcarpeta)) throw new InvalidOperationException("Carpeta de destino no permitida.");
        var ext = Path.GetExtension(nombreOriginal).ToLowerInvariant();
        if (!Extensiones.Contains(ext)) throw new InvalidOperationException("Formato no permitido. Use JPG, PNG o WEBP.");
        if (longitud > MaxBytes) throw new InvalidOperationException("La foto supera los 5 MB.");

        var carpeta = Path.Combine(env.WebRootPath, "uploads", subcarpeta);
        Directory.CreateDirectory(carpeta);
        var nombre = $"{Guid.NewGuid():N}{ext}";
        var ruta = Path.Combine(carpeta, nombre);

        await using (var fs = File.Create(ruta)) await contenido.CopyToAsync(fs, ct);

        if (!EsImagenReal(ruta)) { File.Delete(ruta); throw new InvalidOperationException("El archivo no es una imagen válida."); }
        return $"/uploads/{subcarpeta}/{nombre}";
    }

    public bool EsRutaValida(string? ruta, string subcarpeta)
    {
        var prefijo = $"/uploads/{subcarpeta}/";
        if (string.IsNullOrWhiteSpace(ruta) || !ruta.StartsWith(prefijo)) return false;
        var nombre = ruta[prefijo.Length..];
        return NombreGenerado().IsMatch(nombre) && File.Exists(Path.Combine(env.WebRootPath, "uploads", subcarpeta, nombre));
    }

    public void Eliminar(string? rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa) || !rutaRelativa.StartsWith("/uploads/")) return;
        var fisica = Path.GetFullPath(Path.Combine(env.WebRootPath, rutaRelativa.TrimStart('/')));
        if (fisica.StartsWith(Path.GetFullPath(env.WebRootPath)) && File.Exists(fisica)) File.Delete(fisica);
    }

    private static bool EsImagenReal(string ruta)
    {
        Span<byte> h = stackalloc byte[12];
        using var fs = File.OpenRead(ruta);
        if (fs.Read(h) < 12) return false;
        var jpg = h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
        var png = h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
        var webp = h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F' && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P';
        return jpg || png || webp;
    }
}
