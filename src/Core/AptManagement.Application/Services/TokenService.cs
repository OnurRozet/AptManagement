using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Interfaces;
using AptManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Services
{
    public class TokenService(IConfiguration configuration) : ITokenService
    {
        private readonly IConfiguration _configuration = configuration;

        public TokenResponse CreateToken(AppUser user)
        {
            // 1. Token içine gömülecek bilgiler (Claims)
            // Kimlik kartının üzerindeki yazılar gibi düşünün.
            var claims = new List<Claim>
        {
            // Kullanıcının ID'si (Guid)
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
            
            // Bizim için en önemli bilgi: Daire Numarası (UserName'de tutuyorduk)
            new Claim(ClaimTypes.Name, user.UserName), 
            
            // Yönetici mi? Bu bilgiye göre ön yüzde menüleri açıp kapatacağız.
            new Claim("IsManager", user.IsManager.ToString())
        };

            // 2. İmzalama Anahtarı (Security Key)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Token Ayarları (Süresi, Kimden geldiği vs.)
            var expiry = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtSettings:ExpireDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            // 4. Token'ı oluştur ve string olarak dön
            var tokenHandler = new JwtSecurityTokenHandler();

            return new TokenResponse
            {
                Token = tokenHandler.WriteToken(token),
                Expiration = expiry,
                ApartmentNumber = user.UserName
            };
        }
    }
}
