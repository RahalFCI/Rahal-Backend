using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.FileStorage
{
    /// <summary>
    /// Local file storage service for storing uploaded files in the Rahal.Api wwwroot folder
    /// Supports image file types (jpg, jpeg, png, gif, webp)
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _storagePath;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB

        public LocalFileStorageService(IWebHostEnvironment webHostEnvironment, ILogger<LocalFileStorageService> logger)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;

            // Store files in Rahal.Api/wwwroot/uploads directory
            _storagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            // Create uploads directory if it doesn't exist
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
                _logger.LogInformation("Created uploads directory at {StoragePath}", _storagePath);
            }
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload failed: File is null or empty");
                    throw new ArgumentException("File is null or empty", nameof(file));
                }

                // Validate file size
                if (file.Length > _maxFileSize)
                {
                    _logger.LogWarning("Upload failed: File size {FileSize} exceeds maximum allowed size {MaxSize}", 
                        file.Length, _maxFileSize);
                    throw new InvalidOperationException($"File size exceeds the maximum allowed size of {_maxFileSize / (1024 * 1024)} MB");
                }

                // Validate file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning("Upload failed: File extension {Extension} is not allowed. Allowed: {AllowedExtensions}", 
                        fileExtension, string.Join(", ", _allowedExtensions));
                    throw new InvalidOperationException($"File type '{fileExtension}' is not allowed. Only image files are supported.");
                }

                // Generate unique filename to prevent conflicts
                var uniqueFileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(_storagePath, uniqueFileName);

                _logger.LogInformation("Uploading file {FileName} to {FilePath}", file.FileName, filePath);

                // Save file to disk
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Return relative URL path
                var relativeUrl = $"/uploads/{uniqueFileName}";

                _logger.LogInformation("File successfully uploaded to {RelativeUrl}", relativeUrl);
                return relativeUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file {FileName}", file.FileName);
                throw;
            }
        }

        public async Task DeleteAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl))
                {
                    _logger.LogWarning("Delete failed: File URL is null or empty");
                    throw new ArgumentException("File URL is null or empty", nameof(fileUrl));
                }

                // Convert relative URL to file path
                var fileName = Path.GetFileName(fileUrl);
                var filePath = Path.Combine(_storagePath, fileName);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Delete failed: File not found at {FilePath}", filePath);
                    throw new FileNotFoundException($"File not found: {fileUrl}");
                }

                _logger.LogInformation("Deleting file from {FilePath}", filePath);

                // Delete the file
                File.Delete(filePath);

                _logger.LogInformation("File successfully deleted from {FilePath}", filePath);

                // Return completed task
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting file {FileUrl}", fileUrl);
                throw;
            }
        }

        /// <summary>
        /// Generates a unique filename to prevent conflicts
        /// Uses timestamp and random string to ensure uniqueness
        /// </summary>
        private static string GenerateUniqueFileName(string originalFileName)
        {
            var fileExtension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var randomString = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
            
            return $"{timestamp}_{randomString}{fileExtension}";
        }
    }
}
