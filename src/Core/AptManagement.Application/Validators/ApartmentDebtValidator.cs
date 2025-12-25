using AptManagement.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Validators
{
    public class ApartmentDebtValidator : AbstractValidator<ApartmentDebt>
    {
        public ApartmentDebtValidator()
        {
            RuleFor(x => x.Amount).NotEmpty().NotNull().GreaterThan(0).WithMessage("Borç miktarı boş olamaz ve sıfırdan büyük olmalıdır");
            RuleFor(x => x.DueDate).NotEmpty().WithMessage("Vade tarihi boş olamaz");
            RuleFor(x => x.ApartmentId).NotEmpty().GreaterThan(0).WithMessage("Daire seçilmelidir");
        }
    }
}

