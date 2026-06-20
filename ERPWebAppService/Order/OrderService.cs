using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Order;

namespace WebApp.Service.Order
{
    public class OrderService : IOrderService
    {
        private readonly IMapper _mapper;
        private readonly WebAppDbContext _dbContext;

        public OrderService(IMapper mapper,
            WebAppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<bool> CreateOrder(List<CreateOrderRequestModel> model)
        {
            try
            {
                var request = _mapper.Map<List<OrderHistory>>(model);
                _dbContext.OrderHistory.AddRange(request);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception) { throw; }
        }

        public async Task<dynamic> GetOrders()
        {
            try
            {
               return await _dbContext.OrderHistory.OrderByDescending(x=>x.CreatedOn).ToListAsync();
            }
            catch (Exception) { throw; }
        }
    }
}
