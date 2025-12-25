using AptManagement.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Validators
{
    public class ExpenseValidator : AbstractValidator<Expense>
    {
        public ExpenseValidator()
        {
            RuleFor(x => x.Amount).NotEmpty().NotNull().GreaterThan(0).WithMessage("Gider miktarı boş olamaz ve sıfırdan büyük olmalıdır");
            RuleFor(x => x.ExpenseDate).NotEmpty().WithMessage("Gider tarihi boş olamaz");
            RuleFor(x => x.ExpenseCategoryId).NotEmpty().GreaterThan(0).WithMessage("Gider kategorisi seçilmelidir");
        }
    }
}

