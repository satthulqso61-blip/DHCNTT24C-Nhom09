using FluentValidation;
using StockHub.Application.Features.Warehouses.DTOs;

namespace StockHub.Application.Features.Warehouses.Validators;

public sealed class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
{
    public CreateWarehouseRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required.")
            .MinimumLength(2).WithMessage("Warehouse code must be at least 2 characters.")
            .MaximumLength(20).WithMessage("Warehouse code must not exceed 20 characters.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Warehouse code can only contain alphanumeric characters, underscores, and hyphens.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(150).WithMessage("Warehouse name must not exceed 150 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(250).WithMessage("Address must not exceed 250 characters.");
    }
}

public sealed class UpdateWarehouseRequestValidator : AbstractValidator<UpdateWarehouseRequest>
{
    public UpdateWarehouseRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(150).WithMessage("Warehouse name must not exceed 150 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(250).WithMessage("Address must not exceed 250 characters.");
    }
}
