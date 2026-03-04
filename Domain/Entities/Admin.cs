using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities._Common;

namespace Users.Domain.Entities
{
    internal class Admin : User
    {
        public required Guid UserId { get; set; }
    }
}
