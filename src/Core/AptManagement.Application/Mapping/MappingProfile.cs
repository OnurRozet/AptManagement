using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Apartment, ApartmentDto>().ReverseMap();
            CreateMap<DuesSetting, DuesSettingDto>().ReverseMap();
            CreateMap<DuesSetting, DuesSettingResponse>().ReverseMap();
            CreateMap<Income, IncomeDto>().ReverseMap();
            CreateMap<Income, IncomeResponse>().ReverseMap();
            CreateMap<IncomeCategory, IncomeCategoryDto>().ReverseMap();
            CreateMap<IncomeCategory, IncomeCategoryResponse>().ReverseMap();
            CreateMap<Expense, ExpenseDto>().ReverseMap();
            CreateMap<Expense, ExpenseResponse>().ReverseMap();
            CreateMap<ExpenseCategory, ExpenseCategoryDto>().ReverseMap();
            CreateMap<ExpenseCategory, ExpenseCategoryResponse>().ReverseMap();
            CreateMap<ApartmentDebt, ApartmentDebtDto>().ReverseMap();
            CreateMap<ApartmentDebt, ApartmentDebtResponse>().ReverseMap();
        }
    }
}
