using AptManagement.Application.Common.Base.Response;
using AptManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Interfaces
{
    public interface ITokenService
    {
        TokenResponse CreateToken(AppUser user);
    }
}
