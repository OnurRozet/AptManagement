using AptManagement.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Validators
{
    public class ApartmentValidator : AbstractValidator<Apartment>
    {
        public ApartmentValidator()
        {
            RuleFor(x => x.OwnerName).NotEmpty().WithMessage("Daire sahibi boş olamaz");
            RuleFor(x => x.Label).NotEmpty().WithMessage("Geçerli bir daire tanımı giriniz");

        }
    }
}
