using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantSimulation.Domain.Enums;

namespace RestaurantSimulation.Application.DTO
{
    public class OrderItemDto
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public ItemType Type { get; set; }

        public OrderItemDto(string name, int quantity, decimal unitPrice, ItemType type)
        {
            Name = name;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Type = type;
        }
    }
}
