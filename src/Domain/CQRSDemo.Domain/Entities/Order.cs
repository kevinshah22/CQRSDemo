namespace CQRSDemo.Domain.Entities;

public class Order : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalAmount => Quantity * UnitPrice;
    public OrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }
    public Customer? Customer { get; private set; }

    private Order() { }

    public Order(Guid customerId, string productName, int quantity, decimal unitPrice)
    {
        CustomerId = customerId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Status = OrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
        UpdateTimestamp();
    }

    public void UpdateProduct(string productName, int quantity, decimal unitPrice)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a completed or cancelled order.");

        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        UpdateTimestamp();
    }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Completed,
    Cancelled
}
