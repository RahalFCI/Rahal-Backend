using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Events.CheckIns
{
    public record CreateCheckInEventResponse(
    Guid OperationId,
    bool IsSuccess,
    ErrorCode ErrorCode,
    string? Message);
}
