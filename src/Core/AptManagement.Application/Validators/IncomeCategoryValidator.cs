using AptManagement.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Validators
{
    public class IncomeCategoryValidator : AbstractValidator<IncomeCategory>
    {
        public IncomeCategoryValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Gelir kategorisi adı boş olamaz");
        }
    }
}

