using Microsoft.AspNetCore.Http;

namespace MarcadorFaseIIApi.Services.Interfaces
{
    /// <summary>
    /// Almacenamiento de archivos para logos (guardado y eliminación).
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Guarda un logo y devuelve el nombre de archivo persistido.
        /// </summary>
        Task<string> SaveLogoAsync(IFormFile file, CancellationToken ct = default);

        /// <summary>
        /// Elimina un logo existente si aplica (ignora null o vacío).
        /// </summary>
        Task DeleteLogoAsync(string? fileName);
    }
}
