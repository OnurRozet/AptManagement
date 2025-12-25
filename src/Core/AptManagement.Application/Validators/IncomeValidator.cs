using AptManagement.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Validators
{
    public class IncomeValidator : AbstractValidator<Income>
    {
        public IncomeValidator()
        {
            RuleFor(x => x.Amount).NotEmpty().NotNull().GreaterThan(0).WithMessage("Gelir miktarı boş olamaz ve sıfırdan büyük olmalıdır");
            RuleFor(x => x.IncomeDate).NotEmpty().WithMessage("Gelir tarihi boş olamaz");
            RuleFor(x => x.IncomeCategoryId).NotEmpty().GreaterThan(0).WithMessage("Gelir kategorisi seçilmelidir");
            RuleFor(x => x.ApartmentId).NotEmpty().GreaterThan(0).WithMessage("Daire seçilmelidir");
        }
    }
}

