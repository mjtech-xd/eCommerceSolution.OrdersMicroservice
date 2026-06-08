namespace BusinessLogicLayer.DTO;

public record OrderResponse(Guid OrderID, Guid UserID, DateTime OrderDate,decimal TotalBill, List<OrderItemAddRequest> OrderItems)
{
    public OrderResponse(): this(default, default, default,default, default)
    {
        
    }
}