using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs.Explorer;
using Users.Application.DTOs.Vendor;
using Users.Application.Interfaces;
using Users.Domain.Entities;

namespace Users.Application.Mappers
{
    internal class VendorMapper : IUserMapper<Vendor, VendorDto, VendorSummaryDto>
    {
        public VendorDto ToDto(Vendor user) => user.ToDto();
        public VendorSummaryDto ToSummary(Vendor user) => user.ToSummaryDto();
        public Vendor ToEntity(VendorDto dto) => dto.ToEntity();
    }
}
