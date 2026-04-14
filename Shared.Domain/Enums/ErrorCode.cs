using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Enums
{
    public enum ErrorCode
    {
        None = 0,

        // Generic
        UnknownError = 50000,
        InvalidRequest = 40000,

        // Validation / input
        ValidationError = 42200,
        InvalidFormat = 42201,
        InvalidValue = 42202,
        InvalidCredentials = 42203,

        // Resource errors
        NotFound = 40400,
        AlreadyExists = 40901,
        Conflict = 40900,

        // Authorization
        Unauthorized = 40100,
        LockedOut = 40301,
        Forbidden = 40300,
        EmailNotVerified = 40302,


        // State / business rules
        InvalidOperation = 40910,
        BusinessRuleViolation = 42210,

        // Infrastructure
        DatabaseError = 50010,
        ExternalServiceError = 50200,
        Timeout = 50400
    }

}
