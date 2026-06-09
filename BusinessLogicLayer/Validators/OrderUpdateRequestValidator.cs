using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public class OrderUpdateRequestValidator: AbstractValidator<OrderUpdateRequest>
{
    public OrderUpdateRequestValidator()
    {
        RuleFor(temp => temp.OrderID)
            .NotEmpty().WithMessage("OrderID is required.");
        RuleFor(temp => temp.UserID)
            .NotEmpty().WithMessage("UserID is required.");
        RuleFor(temp => temp.OrderDate)
            .NotEmpty().WithMessage("OrderDate is required.");
        RuleFor(temp => temp.OrderItems)
            .NotEmpty().WithMessage("OrderItems is required.");
    }
}