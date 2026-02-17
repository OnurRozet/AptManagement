using AptManagement.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Validators
{
    public class ManagementPeriodValidator : AbstractValidator<ManagementPeriod>
    {
        public ManagementPeriodValidator()
        {
            RuleFor(x => x.ApartmentId).NotEmpty().GreaterThan(0).WithMessage("Daire seçilmelidir");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Başlangıç tarihi boş olamaz");
            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .When(x => x.EndDate.HasValue)
                .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");
        }
    }
}

