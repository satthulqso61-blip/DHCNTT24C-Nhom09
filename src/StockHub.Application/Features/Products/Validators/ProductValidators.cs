using FluentValidation;
using StockHub.Application.Features.Products.DTOs;

namespace StockHub.Application.Features.Products.Validators;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("Product SKU is required.")
            .MinimumLength(3).WithMessage("Product SKU must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Product SKU must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Product SKU can only contain alphanumeric characters, underscores, and hyphens.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Product unit is required.")
            .MaximumLength(20).WithMessage("Product unit must not exceed 20 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");
    }
}

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Product unit is required.")
            .MaximumLength(20).WithMessage("Product unit must not exceed 20 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");
    }
}
