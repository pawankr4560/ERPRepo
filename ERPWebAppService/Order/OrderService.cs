using AutoMapper;
using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Order;

namespace WebApp.Service.Order;

public class OrderService : IOrderService
{
    private readonly IMapper _mapper;
    private readonly MongoDbContext _context;

    public OrderService(IMapper mapper, MongoDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<bool> CreateOrder(List<CreateOrderRequestModel> model)
    {
        var orders = _mapper.Map<List<OrderHistory>>(model);
        var now = DateTime.UtcNow;
        foreach (var order in orders)
        {
            if (order.Id == Guid.Empty) order.Id = Guid.NewGuid();
            order.CreatedOn = now;
            order.IsActive = true;
            order.IsDeleted = false;
        }

        if (orders.Count > 0)
            await _context.OrderHistory.InsertManyAsync(orders);
        return true;
    }

    public async Task<dynamic> GetOrders() =>
        await _context.OrderHistory
            .Find(x => !x.IsDeleted)
            .SortByDescending(x => x.CreatedOn)
            .ToListAsync();
}
