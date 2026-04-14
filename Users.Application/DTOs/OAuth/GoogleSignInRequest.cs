using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Users.Application.DTOs.OAuth
{
    public record GoogleSignInRequest(string IdToken);
}
