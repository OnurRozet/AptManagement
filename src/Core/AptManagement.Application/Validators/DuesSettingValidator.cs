using AptManagement.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Validators
{
    public class DuesSettingValidator : AbstractValidator<DuesSetting>
    {
        public DuesSettingValidator()
        {
            RuleFor(x=>x.Amount).NotEmpty().NotNull().WithMessage("Aidat miktarı boş olamaz");
            RuleFor(x=>x.Description).NotEmpty().NotNull().WithMessage("Aidat açıklaması boş olamaz");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Aidat başlangıç tarihi boş olamaz");
            RuleFor(x => x.EndDate).NotEmpty().WithMessage("Aidat sona erme tarihi boş olamaz");
        }
    }
}
