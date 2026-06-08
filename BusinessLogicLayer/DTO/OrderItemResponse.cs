namespace BusinessLogicLayer.DTO;

public record OrderItemUpdateResponse(Guid ProductID, decimal UnitPrice, int Quantity, decimal TotalPrice)
{
    public OrderItemUpdateResponse(): this(default, default, default, default)
    {

    }
}