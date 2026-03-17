using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities._Common;
using Users.Domain.Entities._Common;

namespace Users.Domain.Entities
{

    public class AdminProfile : BaseAuditableEntity
    {

        public required Guid UserId { get; set; }


        public required User User { get; set; }


    }
}
