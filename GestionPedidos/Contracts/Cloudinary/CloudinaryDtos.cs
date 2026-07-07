using System.Text.Json.Serialization;

namespace GestionPedidos.Contracts.Cloudinary;

public record CloudinaryFolderDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("fileCount")] int FileCount = 0,
    [property: JsonPropertyName("lastModified")] DateTimeOffset? LastModified = null
);

public record CloudinaryFolderRequestDto(
    [property: JsonPropertyName("folder")] string Folder
);

public record CloudinaryResourceDto(
    [property: JsonPropertyName("public_id")] string PublicId,
    [property: JsonPropertyName("resource_type")] string ResourceType,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("bytes")] long Bytes,
    [property: JsonPropertyName("width")] int? Width,
    [property: JsonPropertyName("height")] int? Height,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("secure_url")] string SecureUrl,
    [property: JsonPropertyName("folder")] string Folder,
    [property: JsonPropertyName("original_filename")] string OriginalFilename,
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("tags")] List<string> Tags
);

public record CloudinaryUploadResponseDto(
    [property: JsonPropertyName("public_id")] string PublicId,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("signature")] string Signature,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("resource_type")] string ResourceType,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("tags")] List<string> Tags,
    [property: JsonPropertyName("bytes")] long Bytes,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("etag")] string Etag,
    [property: JsonPropertyName("placeholder")] bool Placeholder,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("secure_url")] string SecureUrl,
    [property: JsonPropertyName("folder")] string Folder,
    [property: JsonPropertyName("original_filename")] string OriginalFilename,
    [property: JsonPropertyName("api_key")] string ApiKey
);

public record CloudinaryStatsDto(
    [property: JsonPropertyName("folder")] string Folder,
    [property: JsonPropertyName("fileCount")] int FileCount,
    [property: JsonPropertyName("totalSize")] long TotalSize
);
