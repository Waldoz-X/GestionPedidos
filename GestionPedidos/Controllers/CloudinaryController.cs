using GestionPedidos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

namespace GestionPedidos.Controllers;

public class CloudinaryUploadRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
    
    [Required]
    public string Folder { get; set; } = null!;
    
    public string? Tags { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class CloudinaryController(ICloudinaryService cloudinaryService) : ControllerBase
{
    [HttpGet("folders")]
    public async Task<IActionResult> GetFolders()
    {
        try
        {
            var folders = await cloudinaryService.ListFoldersAsync();
            return Ok(folders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("folders")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateFolder([FromBody] GestionPedidos.Contracts.Cloudinary.CloudinaryFolderRequestDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Folder))
            {
                return BadRequest(new { message = "Se requiere el parámetro 'folder' para crear una carpeta." });
            }
            var success = await cloudinaryService.CreateFolderAsync(request.Folder);
            if (success)
            {
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false, message = "No se pudo crear la carpeta." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("folders/{*folder}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFolder(string folder)
    {
        try
        {
            if (string.IsNullOrEmpty(folder))
            {
                return BadRequest(new { message = "Se requiere el parámetro 'folder' para eliminar una carpeta." });
            }
            var success = await cloudinaryService.DeleteFolderAsync(folder);
            if (success)
            {
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false, message = "No se pudo eliminar la carpeta." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("resources")]
    public async Task<IActionResult> GetResources([FromQuery] string prefix)
    {
        try
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return BadRequest(new { message = "Se requiere el parámetro 'prefix' para listar recursos." });
            }
            var resources = await cloudinaryService.ListResourcesAsync(prefix);
            return Ok(resources);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin")] // Only Admin can upload images
    public async Task<IActionResult> UploadImage([FromForm] CloudinaryUploadRequest request)
    {
        try
        {
            var file = request.File;
            var folder = request.Folder;
            var tags = request.Tags;

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No se proporcionó un archivo válido." });

            // Basic validation for image types
            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new { message = "El archivo debe ser una imagen." });
            }

            var result = await cloudinaryService.UploadImageAsync(file, folder, tags);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("resources/{*publicId}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete images
    public async Task<IActionResult> DeleteResource(string publicId)
    {
        try
        {
            var success = await cloudinaryService.DeleteResourceAsync(publicId);
            if (success)
            {
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false, message = "No se pudo eliminar el recurso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("folders/{folder}/stats")]
    public async Task<IActionResult> GetFolderStats(string folder)
    {
        try
        {
            var stats = await cloudinaryService.GetFolderStatsAsync(folder);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
