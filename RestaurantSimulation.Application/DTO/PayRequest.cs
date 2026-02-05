using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSimulation.Application.DTO
{
    public class PayRequest
    {
        public int OrderId { get; set; }
        public decimal PaidAmount { get; set; }

        public PayRequest(int orderId, decimal paidAmount)
        {
            OrderId = orderId;
            PaidAmount = paidAmount;
        }
    }
}
