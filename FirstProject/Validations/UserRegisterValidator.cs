using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FirstProject.Models;

namespace FirstProject.Data.Validations
{
    public class UserRegisterValidator : AbstractValidator<RegistrationModel>
    {
        public UserRegisterValidator()
        {
            RuleFor(x => x.Username)
                .MaximumLength(50)
                .NotEmpty().WithMessage("Username is required");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.PasswordConfirm)
                .Equal(x => x.Password).WithMessage("Password do not match");
        }
    }
}

