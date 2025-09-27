using Ecom.Application.Products.Request;
using FluentValidation;

namespace Ecom.Application.Products.Validators
{
    public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TaxRate).InclusiveBetween(0, 100);
        }
    }
}
