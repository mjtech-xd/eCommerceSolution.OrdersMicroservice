using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public class OrderAddRequestValidator : AbstractValidator<OrderAddRequest>
{
    public OrderAddRequestValidator()
    {
        //UserID
        RuleFor(temp => temp.UserID)
            .NotEmpty().WithMessage("UserID is required.");
        RuleFor(temp => temp.OrderDate)
            .NotEmpty().WithMessage("OrderDate is required.");
        RuleFor(temp => temp.OrderItems)
            .NotEmpty().WithMessage("OrderItems is required.");
    }
}