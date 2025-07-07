using Dapper;
using Dapper.Contrib.Extensions;
using ProductApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApp.Repositories
{
    public class OrderRepository
    {
        private readonly IDbConnection _db;

        public OrderRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> CreateOrderAsync(Order order)
        {
            return await _db.InsertAsync(order);
        }

        public async Task CreateOrderDetailAsync(OrderDetails orderDetail)
        {
            await _db.InsertAsync(orderDetail);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _db.GetAllAsync<Order>();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _db.GetAsync<Order>(orderId);
        }

        public async Task<IEnumerable<OrderDetails>> GetDetailsByOrderIdAsync(int orderId)
        {
            var allDetails = await _db.GetAllAsync<OrderDetails>();
            return allDetails.Where(d => d.OrderId == orderId);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            await _db.UpdateAsync(order);
        }
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            var allOrders = await _db.GetAllAsync<Order>();
            return allOrders.Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate);
        }
    }
}