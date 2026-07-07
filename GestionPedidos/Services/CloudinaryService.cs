using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GestionPedidos.Contracts.Cloudinary;
using Microsoft.AspNetCore.Http;

namespace GestionPedidos.Services;

public interface ICloudinaryService
{
    Task<IEnumerable<CloudinaryFolderDto>> ListFoldersAsync();
    Task<bool> CreateFolderAsync(string folderPath);
    Task<bool> DeleteFolderAsync(string folderPath);
    Task<IEnumerable<CloudinaryResourceDto>> ListResourcesAsync(string prefix);
    Task<CloudinaryUploadResponseDto> UploadImageAsync(IFormFile file, string folder, string? tags);
    Task<bool> DeleteResourceAsync(string publicId);
    Task<CloudinaryStatsDto> GetFolderStatsAsync(string folder);
}

public class CloudinaryService(Cloudinary cloudinary) : ICloudinaryService
{
    public async Task<IEnumerable<CloudinaryFolderDto>> ListFoldersAsync()
    {
        var result = await cloudinary.RootFoldersAsync();
        if (result.Error != null)
        {
            throw new Exception($"Error al listar carpetas en Cloudinary: {result.Error.Message}");
        }

        if (result.Folders == null)
        {
            return new List<CloudinaryFolderDto>();
        }

        return result.Folders.Select(f => new CloudinaryFolderDto(
            Name: f.Name,
            Path: f.Path,
            FileCount: 0,
            LastModified: null
        ));
    }

    public async Task<bool> CreateFolderAsync(string folderPath)
    {
        var result = await cloudinary.CreateFolderAsync(folderPath);
        if (result.Error != null)
        {
            throw new Exception($"Error al crear la carpeta en Cloudinary: {result.Error.Message}");
        }
        return true;
    }

    public async Task<bool> DeleteFolderAsync(string folderPath)
    {
        var result = await cloudinary.DeleteFolderAsync(folderPath);
        if (result.Error != null)
        {
            throw new Exception($"Error al eliminar la carpeta en Cloudinary: {result.Error.Message}");
        }
        return true;
    }

    public async Task<IEnumerable<CloudinaryResourceDto>> ListResourcesAsync(string prefix)
    {
        var parameters = new ListResourcesByPrefixParams
        {
            Prefix = prefix,
            Type = "upload",
            MaxResults = 500
        };

        var result = await cloudinary.ListResourcesAsync(parameters);
        if (result.Error != null)
        {
            throw new Exception($"Error al listar recursos en Cloudinary: {result.Error.Message}");
        }

        if (result.Resources == null)
        {
            return new List<CloudinaryResourceDto>();
        }

        return result.Resources.Select(r => {
            int lastSlash = r.PublicId.LastIndexOf('/');
            string folderName = lastSlash >= 0 ? r.PublicId.Substring(0, lastSlash) : "";
            string fileName = lastSlash >= 0 ? r.PublicId.Substring(lastSlash + 1) : r.PublicId;

            return new CloudinaryResourceDto(
                PublicId: r.PublicId,
                ResourceType: r.ResourceType,
                Type: r.Type,
                CreatedAt: r.CreatedAt,
                Bytes: r.Bytes,
                Width: r.Width,
                Height: r.Height,
                Url: r.Url?.ToString() ?? "",
                SecureUrl: r.SecureUrl?.ToString() ?? "",
                Folder: folderName,
                OriginalFilename: fileName,
                Format: r.Format ?? "img",
                Tags: []
            );
        });
    }

    public async Task<CloudinaryUploadResponseDto> UploadImageAsync(IFormFile file, string folder, string? tags)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("El archivo está vacío o no es válido.");

        // Validate size (10MB limit)
        if (file.Length > 10 * 1024 * 1024)
            throw new ArgumentException("El tamaño del archivo excede el límite de 10 MB.");

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            Tags = string.IsNullOrEmpty(tags) ? null : tags
        };

        var result = await cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            throw new Exception($"Error al subir imagen a Cloudinary: {result.Error.Message}");
        }

        return new CloudinaryUploadResponseDto(
            PublicId: result.PublicId,
            Version: result.Version,
            Signature: result.Signature,
            Width: result.Width,
            Height: result.Height,
            Format: result.Format,
            ResourceType: result.ResourceType,
            CreatedAt: result.CreatedAt.ToString("o"),
            Tags: result.Tags?.ToList() ?? [],
            Bytes: result.Bytes,
            Type: result.Type,
            Etag: result.Etag,
            Placeholder: result.Placeholder,
            Url: result.Url?.ToString() ?? "",
            SecureUrl: result.SecureUrl?.ToString() ?? "",
            Folder: folder,
            OriginalFilename: result.OriginalFilename,
            ApiKey: ""
        );
    }

    public async Task<bool> DeleteResourceAsync(string publicId)
    {
        var deletionParams = new DeletionParams(publicId);
        var result = await cloudinary.DestroyAsync(deletionParams);

        if (result.Error != null)
        {
            throw new Exception($"Error al eliminar imagen en Cloudinary: {result.Error.Message}");
        }

        return result.Result == "ok";
    }

    public async Task<CloudinaryStatsDto> GetFolderStatsAsync(string folder)
    {
        var resources = await ListResourcesAsync(folder);
        int fileCount = resources.Count();
        long totalSize = resources.Sum(r => r.Bytes);

        return new CloudinaryStatsDto(folder, fileCount, totalSize);
    }
}
