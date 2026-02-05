using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSimulation.Domain.Enums;

public enum OrderStatus
{
    New = 0,
    Queued = 1,
    InPreparation = 2,
    Ready = 3,
    Delivered = 4,
    Paid = 5
}

