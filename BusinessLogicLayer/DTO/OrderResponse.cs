namespace BusinessLogicLayer.DTO;

public record OrderResponse(
    Guid OrderID,
    Guid UserID,
    DateTime OrderDate,
    decimal TotalBill,
    List<OrderItemResponse> OrderItems,
    string? UserPersonName,
    string? Email)
{
    public OrderResponse() : this(default, default, default, default, default, default, default)
    {
    }
}