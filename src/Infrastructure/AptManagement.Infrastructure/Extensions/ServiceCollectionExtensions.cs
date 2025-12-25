using AptManagement.Application.Interfaces;
using AptManagement.Application.Mapping;
using AptManagement.Application.Services;
using AptManagement.Application.Validators;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Interfaces;
using AptManagement.Infrastructure.Context;
using AptManagement.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                db.UseSqlServer(connectionString);
            });

            // FluentValidation - Assembly taraması yerine sadece belirli validator tipini kullan  
            services.AddValidatorsFromAssembly(typeof(ApartmentValidator).Assembly);

            // AutoMapper - Corrected to use the overload that accepts a configuration action  
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IApartmentService, ApartmentService>();
            services.AddScoped<IDuesSettingService, DuesSettingService>();
            services.AddScoped<IIncomeService, IncomeService>();
            services.AddScoped<IIncomeCategoryService, IncomeCategoryService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
            services.AddScoped<IApartmentDebtService, ApartmentDebtService>();

            return services;
        }
    }
}
