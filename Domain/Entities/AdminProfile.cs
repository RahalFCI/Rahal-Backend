using System;
using System.Collections.Generic;
using System.Text;
using Shared.Domain.Entities;
using Users.Domain.Entities._Common;

namespace Users.Domain.Entities
{

    public class AdminProfile : BaseEntity
    {

        public required Guid UserId { get; set; }


        public required User User { get; set; }


    }
}
