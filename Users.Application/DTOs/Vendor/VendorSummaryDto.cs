using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Domain.Enums;

namespace Users.Application.DTOs.Vendor
{
    public record VendorSummaryDto(
        string CountryCode,
        string Address,
        string AddressUrl,
        int CategoryId,
        bool IsApproved) : BaseUserSummaryDto
    {
        public VendorSummaryDto() : this(string.Empty, string.Empty, string.Empty, default, false) { }
    }

}
