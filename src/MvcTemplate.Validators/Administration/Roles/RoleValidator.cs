using Microsoft.AspNetCore.Mvc.ModelBinding;
using MvcTemplate.Data;
using MvcTemplate.Objects;
using MvcTemplate.Resources;
using System;
using System.Linq;

namespace MvcTemplate.Validators
{
    public class RoleValidator : BaseValidator, IRoleValidator
    {
        public RoleValidator(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public Boolean CanCreate(RoleView view)
        {
            Boolean isValid = IsUniqueTitle(0, view.Title);
            isValid &= ModelState.IsValid;

            return isValid;
        }
        public Boolean CanEdit(RoleView view)
        {
            Boolean isValid = IsUniqueTitle(view.Id, view.Title);
            isValid &= ModelState.IsValid;

            return isValid;
        }

        private Boolean IsUniqueTitle(Int64 id, String? title)
        {
            Boolean isUnique = !UnitOfWork
                .Select<Role>()
                .Any(role =>
                    role.Id != id &&
                    role.Title.ToLower() == (title ?? "").ToLower());

            if (!isUnique)
                ModelState.AddModelError<RoleView>(role => role.Title, Validation.For<RoleView>("UniqueTitle"));

            return isUnique;
        }
    }
}
