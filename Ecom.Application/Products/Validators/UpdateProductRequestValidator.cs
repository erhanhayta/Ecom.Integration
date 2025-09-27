using Ecom.Application.Products.Request;
using FluentValidation;

namespace Ecom.Application.Products.Validators
{
    public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            Include(new CreateProductRequestValidator());
        }
    }
}
