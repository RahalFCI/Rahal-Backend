using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTOs.EmailVerification
{
    public class VerifyOtpRequest
    {
        public required string Email { get; set; }
        public required string Otp { get; set; }
    }

    public class ResendOtpRequest
    {
        public required string Email { get; set; }
    }
}
