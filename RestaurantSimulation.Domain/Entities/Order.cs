using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantSimulation.Domain.Enums;

namespace RestaurantSimulation.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; }

    public Order(int id, int tableId)
    {
        Id = id;
        TableId = tableId;
        CreatedAt = DateTime.Now;
        Status = OrderStatus.New;
        Items = new List<OrderItem>();
    }

    public void AddItem(OrderItem item)
    {
        Items.Add(item);
    }

    public decimal TotalAmount()
    {
        return Items.Sum(i => i.TotalPrice());
    }

    public bool HasFood()
    {
        return Items.Any(i => i.Type == ItemType.Food);
    }

    public bool HasDrink()
    {
        return Items.Any(i => i.Type == ItemType.Drink);
    }
}

