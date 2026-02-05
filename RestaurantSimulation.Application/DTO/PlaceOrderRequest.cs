using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSimulation.Application.DTO
{
    public class PlaceOrderRequest
    {
        public int TableId { get; set; }
        public List<OrderItemDto> Items { get; set; }

        public PlaceOrderRequest(int tableId, List<OrderItemDto> items)
        {
            TableId = tableId;
            Items = items;
        }
    }
}
