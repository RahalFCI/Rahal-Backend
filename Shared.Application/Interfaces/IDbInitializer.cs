using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Interfaces
{
    public interface IDbInitializer
    {
        Task SeedAsync();
    }
}
