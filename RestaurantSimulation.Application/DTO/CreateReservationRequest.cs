using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSimulation.Application.DTO
{
    public class CreateReservationRequest
    {
        public int TableId { get; set; }
        public string GuestName { get; set; }
        public int GuestCount { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public CreateReservationRequest(
            int tableId,
            string guestName,
            int guestCount,
            DateTime from,
            DateTime to)
        {
            TableId = tableId;
            GuestName = guestName;
            GuestCount = guestCount;
            From = from;
            To = to;
        }
    }
}
