using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public class OrderItemUpdateRequestValidator: AbstractValidator<OrderItemUpdateRequest>
{
    public OrderItemUpdateRequestValidator()
    {
        RuleFor(temp => temp.ProductID)
            .NotEmpty().WithMessage("ProductID is required.");
        RuleFor(temp => temp.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        RuleFor(temp => temp.UnitPrice)
            .GreaterThan(0).WithMessage("UnitPrice must be greater than 0.");
    }
}