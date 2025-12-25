using AptManagement.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Validators
{
    public class ExpenseCategoryValidator : AbstractValidator<ExpenseCategory>
    {
        public ExpenseCategoryValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Gider kategorisi adı boş olamaz");
        }
    }
}

