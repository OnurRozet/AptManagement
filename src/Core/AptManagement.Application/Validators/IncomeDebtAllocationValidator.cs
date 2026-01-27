using AptManagement.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Validators
{
    public class IncomeDebtAllocationValidator : AbstractValidator<IncomeDebtAllocation>
    {
        public IncomeDebtAllocationValidator()
        {
            RuleFor(x => x.IncomeId).NotEmpty().GreaterThan(0).WithMessage("Gelir seçilmelidir");
            RuleFor(x => x.ApartmentDebtId).NotEmpty().GreaterThan(0).WithMessage("Borç seçilmelidir");
            RuleFor(x => x.AllocatedAmount).NotEmpty().NotNull().GreaterThan(0).WithMessage("Ayrılan tutar boş olamaz ve sıfırdan büyük olmalıdır");
        }
    }
}

