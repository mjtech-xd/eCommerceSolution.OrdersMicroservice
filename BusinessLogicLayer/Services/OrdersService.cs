using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services;

public class OrdersService(
    IOrdersRepository ordersRepository,
    IMapper mapper,
    IValidator<OrderAddRequest> orderAddRequestValidator,
    IValidator<OrderItemAddRequest> orderItemAddRequestValidator,
    IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
    IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator) : IOrdersService
{
    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Order?> orders = await ordersRepository.GetOrders();
        
        IEnumerable<OrderResponse?> ordersResponse = mapper.Map<IEnumerable<OrderResponse?>>(orders);
        return ordersResponse.ToList();
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await ordersRepository.GetOrdersByCondition(filter);
        
        IEnumerable<OrderResponse?> ordersResponse = mapper.Map<IEnumerable<OrderResponse?>>(orders);
        return ordersResponse.ToList();
    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
         Order? order = await ordersRepository.GetOrderByCondition(filter);
         if (order is null)             
             return null;
         OrderResponse? orderResponse = mapper.Map<OrderResponse?>(order);
         return orderResponse;
    }

    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        //Check for null parameters
        if (orderAddRequest == null)
            throw new ArgumentNullException(nameof(orderAddRequest));
        //Validator
        ValidationResult orderAddRequestValidationResult =
            await orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!orderAddRequestValidationResult.IsValid)
        {
            string errors = string.Join(",", orderAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        //Validate order items using fluent validation  
        foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult =
                await orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);
            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(",",
                    orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }
        }

        //TO DO: Add logic for the UserID exists in user microservice 
        Order orderInput = mapper.Map<Order>(orderAddRequest);
        //Generate OrderID and TotalBill
        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }

        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);
        Order? order = await ordersRepository.AddOrder(orderInput);
        if (order is null)
            return null;
        OrderResponse? addedOrderResponse = mapper.Map<OrderResponse?>(order);
        return addedOrderResponse;
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        //Check for null parameters
        if (orderUpdateRequest == null)
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        
        //Validator
        ValidationResult orderUpdateRequestValidationResult =
            await orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(",", orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        //Validate order items using fluent validation  
        foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult =
                await orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);
            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(",",
                    orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }
        }

        //TO DO: Add logic for the UserID exists in user microservice 
        Order orderInput = mapper.Map<Order>(orderUpdateRequest);
        
        //Generate  and TotalBill
        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }

        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);
        Order? order = await ordersRepository.UpdateOrder(orderInput);
        if (order is null)
            return null;
        OrderResponse? updatedOrderResponse = mapper.Map<OrderResponse?>(order);
        return updatedOrderResponse;
    }

    public async Task<bool?> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        Order? existingOrder =  await ordersRepository.GetOrderByCondition(filter);
        if (existingOrder is null)
            return false;
        bool isDeleted =  await ordersRepository.DeleteOrder(orderID);
        return isDeleted;
    }
}