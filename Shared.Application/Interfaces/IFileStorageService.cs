using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken);
        Task DeleteAsync(string fileUrl, CancellationToken cancellationToken);
    }
}
