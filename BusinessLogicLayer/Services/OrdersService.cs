using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
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
    IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator,
    UsersMicroserviceClient usersMicroserviceClient,
    ProductsMicroserviceClient productsMicroserviceClient) : IOrdersService
{
    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Order?> orders = await ordersRepository.GetOrders();

        IEnumerable<OrderResponse?> ordersResponse = mapper.Map<IEnumerable<OrderResponse?>>(orders);

        //TO DO: Load ProductName and category in each OrderItem
        foreach (OrderResponse? orderResponse in ordersResponse)
        {
            if (orderResponse is null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO =
                    await productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                if (productDTO is null)
                    continue;
                mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }

            //TO DO: Load UserPersonName and email from users microservice
            UserDTO? userDTO = await usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (userDTO is not null)
            {
                mapper.Map<UserDTO, OrderResponse>(userDTO, orderResponse);
            }
        }

        return ordersResponse.ToList();
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await ordersRepository.GetOrdersByCondition(filter);

        IEnumerable<OrderResponse?> ordersResponse = mapper.Map<IEnumerable<OrderResponse?>>(orders);

        //TO DO: Load ProductName and category in each OrderItem
        foreach (OrderResponse? orderResponse in ordersResponse)
        {
            if (orderResponse is null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO =
                    await productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                if (productDTO is null)
                    continue;
                mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }

            //TO DO: Load UserPersonName and email from users microservice
            UserDTO? userDTO = await usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (userDTO is not null)
            {
                mapper.Map<UserDTO, OrderResponse>(userDTO, orderResponse);
            }
        }

        return ordersResponse.ToList();
    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order? order = await ordersRepository.GetOrderByCondition(filter);
        if (order is null)
            return null;
        OrderResponse? orderResponse = mapper.Map<OrderResponse?>(order);

        //TO DO: Load ProductName and category in each OrderItem
        if (orderResponse is not null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO =
                    await productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                if (productDTO is null)
                    continue;
                mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
        }

        //TO DO: Load UserPersonName and email from users microservice
        if (orderResponse is not null)
        {
            UserDTO? userDTO = await usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (userDTO is not null)
            {
                mapper.Map<UserDTO, OrderResponse>(userDTO, orderResponse);
            }
        }

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

        // Collect product details for validated order items
        List<ProductDTO> products = new List<ProductDTO>();
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

            //TO DO: Add logic for the ProductID exists in product microservice 
            ProductDTO? product = await productsMicroserviceClient.GetProductByProductID(orderItemAddRequest.ProductID);
            if (product is null)
                throw new ArgumentException($"ProductId: {orderItemAddRequest.ProductID} is invalid");
            products.Add(product);
        }

        //TO DO: Add logic for the UserID exists in user microservice 
        UserDTO? user = await usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);
        if (user is null)
            throw new ArgumentException("UserId is invalid");

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

        //TO DO: Load ProductName and category in each OrderItem
        if (addedOrderResponse is not null)
        {
            foreach (OrderItemResponse orderItemResponse in addedOrderResponse.OrderItems)
            {
                // Find the matching product from the previously fetched list
                ProductDTO? productDto = products.FirstOrDefault(x => x.ProductID == orderItemResponse.ProductID);
                if (productDto is null)
                    continue;
                // Map non-null product details into the existing order item response
                mapper.Map(productDto, orderItemResponse);
            }
        }

        //TO DO: Load UserPersonName and email from users microservice
        if (addedOrderResponse is not null)
        {
            if (user is not null)
            {
                mapper.Map<UserDTO, OrderResponse>(user, addedOrderResponse);
            }
        }

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
            string errors = string.Join(",",
                orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        List<ProductDTO> products = new List<ProductDTO>();
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

            //TO DO: Add logic for the ProductID exists in product microservice 
            ProductDTO? product =
                await productsMicroserviceClient.GetProductByProductID(orderItemUpdateRequest.ProductID);
            if (product is null)
                throw new ArgumentException($"ProductId: {orderItemUpdateRequest.ProductID} is invalid");
            products.Add(product);
        }

        //TO DO: Add logic for the UserID exists in user microservice 
        UserDTO? user = await usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);
        if (user is null)
            throw new ArgumentException("UserId is invalid");
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

        //TO DO: Load ProductName and category in each OrderItem
        if (updatedOrderResponse is not null)
        {
            foreach (OrderItemResponse orderItemResponse in updatedOrderResponse.OrderItems)
            {
                // Find the matching product from the previously fetched list
                ProductDTO? productDto = products.FirstOrDefault(x => x.ProductID == orderItemResponse.ProductID);
                if (productDto is null)
                    continue;
                // Map non-null product details into the existing order item response
                mapper.Map(productDto, orderItemResponse);
            }
        }

        //TO DO: Load UserPersonName and email from users microservice
        if (updatedOrderResponse is not null)
        {
            if (user is not null)
            {
                mapper.Map<UserDTO, OrderResponse>(user, updatedOrderResponse);
            }
        }

        return updatedOrderResponse;
    }

    public async Task<bool?> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        Order? existingOrder = await ordersRepository.GetOrderByCondition(filter);
        if (existingOrder is null)
            return false;
        bool isDeleted = await ordersRepository.DeleteOrder(orderID);
        return isDeleted;
    }
}