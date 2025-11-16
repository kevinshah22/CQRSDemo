using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Repositories;
using CQRSDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CQRSDemo.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Where(o => o.Status == status)
            .ToListAsync(cancellationToken);
    }
}
