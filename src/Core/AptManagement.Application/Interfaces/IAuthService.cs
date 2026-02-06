using AptManagement.Application.Common;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<bool>> RegisterAsync(RegisterDto dto);
        Task<ServiceResult<TokenResponse>> LoginAsync(LoginDto dto);
        Task LogoutAsync();
        Task<ServiceResult<UserResponse>> GetUserInfoAsync(string apartmentNumber);
    }
}
