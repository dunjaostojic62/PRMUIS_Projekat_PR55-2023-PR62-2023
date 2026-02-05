using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantSimulation.Domain.Enums;

namespace RestaurantSimulation.Domain.Entities;

public class Table
{
    public int Id { get; set; }
    public int Capacity { get; set; }
    public int CurrentGuests { get; set; }
    public TableStatus Status { get; set; }

    public Table(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        CurrentGuests = 0;
        Status = TableStatus.Free;
    }

    public bool CanSeat(int guests)
    {
        if (Status != TableStatus.Free)
            return false;

        if (guests > Capacity)
            return false;

        return true;
    }

    public void Occupy(int guests)
    {
        CurrentGuests = guests;
        Status = TableStatus.Occupied;
    }

    public void Free()
    {
        CurrentGuests = 0;
        Status = TableStatus.Free;
    }
}

