using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersMicroservice.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController(IOrdersService ordersService) : ControllerBase
{
    //Get: /api/orders
    [HttpGet]
    public async Task<IEnumerable<OrderResponse?>> Get()
    {
        List<OrderResponse?> orders = await ordersService.GetOrders();
        return orders;
    }
    
    //Get: /api/orders/search/orderId/{orderID}
    [HttpGet("/search/orderId/{orderID}")]
    public async Task<OrderResponse?> GetOrderByOrderID(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderID, orderID);
        OrderResponse? order = await ordersService.GetOrderByCondition(filter);
        return order;
    }
    
    //Get: /api/orders/search/productId/{productID}
    [HttpGet("/search/productId/{productID}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductID(Guid productID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(x => x.OrderItems,
            Builders<OrderItem>.Filter.Eq(x => x.ProductID, productID));
        List<OrderResponse?> orders = await ordersService.GetOrdersByCondition(filter);
        return orders;
    }
    
    //Get: /api/orders/search/orderDate/{orderDate}
    [HttpGet("/search/orderDate/{orderDate}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByOrderDate(DateTime orderDate)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyyy-MM-dd"));
        List<OrderResponse?> orders = await ordersService.GetOrdersByCondition(filter);
        return orders;
    }
    
    //Get: /api/orders/search/userID/{userID}
    [HttpGet("/search/userID/{userID}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByUserId(Guid userID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(x => x.UserID, userID);
        List<OrderResponse?> orders = await ordersService.GetOrdersByCondition(filter);
        return orders;
    }

    [HttpPost]
    public async Task<IActionResult> Post(OrderAddRequest? orderAddRequest)
    {
        if (orderAddRequest is null)
            return BadRequest("Invalid order data");
        OrderResponse? orderResponse = await ordersService.AddOrder(orderAddRequest);
        if (orderResponse is null)
            return Problem("Error in adding order");
        return Created($"api/orders/search/orderId/{orderResponse.OrderID}", orderResponse);
    }
    
    [HttpPut("{orderID}")]
    public async Task<IActionResult> Post(Guid orderID, OrderUpdateRequest? orderUpdateRequest)
    {
        if (orderUpdateRequest is null)
            return BadRequest("Invalid order data");
        if(orderID != orderUpdateRequest.OrderID)
            return BadRequest("Order ID in URL and body do not match");
        
        OrderResponse? orderResponse = await ordersService.UpdateOrder(orderUpdateRequest);
        if (orderResponse is null)
            return Problem("Error in adding order");
        return Ok(orderResponse);
    }
    
    [HttpDelete("{orderID}")]
    public async Task<IActionResult> Delete(Guid orderID)
    {
        
        if(orderID == Guid.Empty)
            return BadRequest("OrderID is required");
        
        bool? isDeleted = await ordersService.DeleteOrder(orderID);
        if(!isDeleted.HasValue)
            return Problem("Error in deleting order");
        return Ok(isDeleted);
    }
}