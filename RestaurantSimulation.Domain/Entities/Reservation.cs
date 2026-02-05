using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSimulation.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string GuestName { get; set; }
    public int GuestCount { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public bool IsActive { get; set; }

    public Reservation(
        int id,
        int tableId,
        string guestName,
        int guestCount,
        DateTime from,
        DateTime to)
    {
        Id = id;
        TableId = tableId;
        GuestName = guestName;
        GuestCount = guestCount;
        From = from;
        To = to;
        IsActive = true;
    }

    public bool IsValidAt(DateTime time)
    {
        if (!IsActive)
            return false;

        if (time < From || time > To)
            return false;

        return true;
    }

    public void Cancel()
    {
        IsActive = false;
    }
}

