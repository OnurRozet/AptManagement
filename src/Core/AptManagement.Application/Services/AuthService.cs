using AptManagement.Application.Common;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Application.Interfaces;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Services
{
    public class AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService) : IAuthService
    {
        public async Task<ServiceResult<TokenResponse>> LoginAsync(LoginDto dto)
        {

            var user = await userManager.FindByNameAsync(dto.ApartmentNumber);

            if (user == null) return ServiceResult<TokenResponse>.Error("Kullanıcı Bulunamadı");

            var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (result.Succeeded)
            {
                var tokenDto = tokenService.CreateToken(user);
                return ServiceResult<TokenResponse>.Success(tokenDto,"Giriş Başarılı");
            }

            return ServiceResult<TokenResponse>.Error("Daire no veya şifre hatalı");

        }

        public async Task LogoutAsync()
        {
            await signInManager.SignOutAsync();
        }

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await userManager.FindByNameAsync(dto.ApartmentNumber);
            if (existingUser != null)
            {
                return ServiceResult<bool>.Error($"Giriş yapmaya çalıştığınız {dto.ApartmentNumber} nolu daire sisteme kayıtlı");
            }

            var user = new AppUser
            {
                FullName = dto.FullName,
                ApartmentNumber = dto.ApartmentNumber,
                ApartmentId = int.Parse(dto.ApartmentNumber),
                UserName = dto.ApartmentNumber,
                Email = $"daire{dto.ApartmentNumber}@site.com", // Dummy email
                IsManager = false // Varsayılan olarak false
               
            };

            var result = await userManager.CreateAsync(user,dto.Password);

            if (result.Succeeded)
            {
                return ServiceResult<bool>.Success(result:true , "Kayıt başarıyla oluşturuldu.");
            }
            else
            {
                // Identity'den gelen hataları (Örn: Şifre çok basit) tek bir string'e çeviriyoruz.
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult<bool>.Error(errors);
            }
        }
    }
}
