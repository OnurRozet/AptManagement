using AptManagement.Application.Interfaces;
using AptManagement.Application.Mapping;
using AptManagement.Application.Services;
using AptManagement.Application.Validators;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Interfaces;
using AptManagement.Infrastructure.Context;
using AptManagement.Infrastructure.Repositories;
using AptManagement.Infrastructure.UnitOfWork;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AptManagementContext>(db =>
            {
                db.UseSqlServer(connectionString, options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    );
                });
            });

            // FluentValidation - Assembly taraması yerine sadece belirli validator tipini kullan  
            services.AddValidatorsFromAssembly(typeof(ApartmentValidator).Assembly);

            // AutoMapper - Corrected to use the overload that accepts a configuration action  
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            //JWT yapısını kuralım
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecurityKey"]))
                };
            });

            //identity Yapısını kraulım
            services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 3; // Sadece test için kısa şifre
            })
            .AddEntityFrameworkStores<AptManagementContext>() // Hangi Context'i kullanacak?
            .AddDefaultTokenProviders();

            // Ensure the correct UnitOfWork class is used
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IApartmentService, ApartmentService>();
            services.AddScoped<IDuesSettingService, DuesSettingService>();
            services.AddScoped<IIncomeService, IncomeService>();
            services.AddScoped<IIncomeCategoryService, IncomeCategoryService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
            services.AddScoped<IApartmentDebtService, ApartmentDebtService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IBankStatementService, BankStatementService>();
            services.AddScoped<IIncomeDebtAllocationService, IncomeDebtAllocationService>();
            services.AddScoped<IManagementPeriodService, ManagementPeriodService>();
            services.AddHostedService<YearlyAutomaticAptDebtsBackgroundService>();

            return services;
        }
    }
}
